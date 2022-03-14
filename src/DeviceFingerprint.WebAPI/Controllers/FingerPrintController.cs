using Device.Fingerprint.WebAPI.Services;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.Fingerprint.WebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class FingerPrintController : ControllerBase
    {
        private readonly ISentryClient sentryClient;
        private readonly IWebHostEnvironment environment;
        private readonly IAppSettings appSettings;
        private readonly IFingerPrintService fingerPrintService;
        private readonly ISubscriberServices subscriberServices;
        private readonly IKafkaServices kafkaServices;

        public FingerPrintController(ISentryClient sentryClient,
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            IFingerPrintService fingerPrintService,
            ISubscriberServices subscriberServices,
            IKafkaServices kafkaServices)
        {
            this.sentryClient = sentryClient;
            this.environment = Environment;
            this.appSettings = appSettings;
            this.fingerPrintService = fingerPrintService;
            this.subscriberServices = subscriberServices;
            this.kafkaServices = kafkaServices;
        }

        [HttpGet("scr/{sessionKey}/{subscriberName}")]
        public ActionResult Script(string sessionKey, string subscriberName)
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/gfp.js";

            if (appSettings.Environment != "DEV")
            {
                scriptPath = $"{environment.ContentRootPath}/Scripts/gfp.min.js";
            }

            var bundleFile = new BundleFile(scriptPath);

            if (!string.IsNullOrWhiteSpace(bundleFile.Content))
            {
                var subscribers = subscriberServices.GetSubscriberByName(subscriberName);

                if (subscribers != null && subscribers.Any())
                {
                    var subscriberId = subscribers
                    .Select(x => x.SubscriberId).First();

                    var bundleContent = new BundleContent(subscriberId, bundleFile.Content, appSettings, sessionKey);
                    return new JavascriptResult(bundleContent.Content);
                }
                else
                {
                    sentryClient.CaptureMessage("Subscriber " + subscriberName + " is not exists");
                }
            }

            return null;
        }

        [HttpGet("conf")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<ScriptConfigModel> AppConfig()
        {
            var JWTService = new JWT(appSettings);

            string jsonString = JsonConvert.SerializeObject(new ScriptConfigModel
            {
                EncryptedKey = appSettings.EncryptPassword,               
                Token = JWTService.GenerateJwtToken()
            });

            return Ok(jsonString);
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("fp")]
        public void FingerPrint([FromBody] string data)
        {
            try
            {
                var scrypto = new Crypto(appSettings.EncryptPassword);
                var decryptedData = scrypto.Decrypt(data);

                var fingerPrint = JsonConvert.DeserializeObject<Models.FingerPrintModel>(decryptedData);
                if (fingerPrint != null && !string.IsNullOrWhiteSpace(fingerPrint.FingerPrint))
                {
                    var fingerPrintModel = new FingerPrintModel
                    {
                        FingerPrintCode = fingerPrint.FingerPrintCode,
                        Attribute = fingerPrint.FingerPrint,
                        SessionKey = fingerPrint.SessionKey,
                        SubscriberId = CheckExistSubscriberById(fingerPrint.SubscriberId),
                        UserAgentAttribute = fingerPrint.UserAgentAttribute,
                        UserAgentCode = fingerPrint.UserAgentCode,
                        CookieCode = fingerPrint.CookieCode,
                        Flagged = int.TryParse(fingerPrint.Flagged, out int output) ? Enum.GetName(typeof(FlaggedStatus), int.Parse(fingerPrint.Flagged)) : "",
                        Url = fingerPrint.Url
                    };

                    kafkaServices.SendFingerprintToKafka(fingerPrintModel);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        [HttpPost("transaction")]
        public void Transaction([FromBody] TransactionDTO data)
        {
            try
            {
                if (data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SessionKey) && !string.IsNullOrWhiteSpace(data.SubscriberName))
                {
                    var transaction = new TransactionModel
                    {
                        SubscriberId = CheckExistSubscriberByName(data.SubscriberName),
                        ClientIp = data.ClientIp,
                        LoginName = data.LoginName,
                        SessionKey = data.SessionKey,
                        TransTime = DateTime.Now,
                        UserAgent = data.UserAgent,
                        Url = data.Url,
                        Action = data.Action,
                        TransKey = data.TransKey,
                        CustId = data.CustId,
                        SubAccId = data.SubAccId
                    };

                    kafkaServices.SendTransactionToKafka(transaction);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);               
            }
        }

        [HttpGet("subscriber/{subscriberId}")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<IEnumerable<string>> GetSubscriberInfo(int subscriberId)
        {
            var subscribers = subscriberServices.GetSubscriberById(subscriberId);

            if (subscribers != null && subscribers.Any())
            {
                var domains = subscribers
                .Select(x => x.SubscriberName);

                return Ok(domains);
            }

            return null;
        }

        [HttpGet("hc")]
        public void HealthCheck()
        {
            // Method intentionally left empty.
        }

        private int CheckExistSubscriberById(int subscriberId)
        {
            var subscribers = subscriberServices.GetSubscriberById(subscriberId);

            if (subscribers != null && subscribers.Any())
            {
                var subscriber = subscribers.FirstOrDefault();

                if (subscriber != null)
                {
                    return subscriber.SubscriberId;
                }
            }

            return -1;
        }

        private int CheckExistSubscriberByName(string subscriberName)
        {
            var subscribers = subscriberServices.GetSubscriberByName(subscriberName);

            if (subscribers != null && subscribers.Any())
            {
                var subscriber = subscribers.FirstOrDefault();

                if (subscriber != null)
                {
                    return subscriber.SubscriberId;
                }
            }

            return -1;
        }
    }
}