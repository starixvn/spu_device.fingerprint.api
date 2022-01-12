using Confluent.Kafka;
using DeviceFingerprint.WebAPI.Services;
using DeviceFingerprint.WebAPI.Ultilities;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using Fps.Device.FingerPrint.Features.FingerPrint.Services;
using Fps.Device.FingerPrint.Features.Subscriber.Services;
using Fps.Device.FingerPrint.Features.Transaction.Services;
using FPS.Device.FingerPrint.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DeviceFingerprint.WebAPI.Controllers
{
    [Route("api/fp")]
    [ApiController]
    public class FingerPrintController : ControllerBase
    {
        private readonly ISentryClient sentryClient;
        private readonly IWebHostEnvironment environment;
        private readonly IAppSettings appSettings;
        private readonly IFingerPrintService fingerPrintService;
        private readonly ITransactionService transactionService;
        private readonly ISubscriberServices subscriberServices;

        public FingerPrintController(ISentryClient sentryClient,
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            IFingerPrintService fingerPrintService,
            ITransactionService transactionService,
            ISubscriberServices subscriberServices)
        {
            this.sentryClient = sentryClient;
            this.environment = Environment;
            this.appSettings = appSettings;
            this.fingerPrintService = fingerPrintService;
            this.transactionService = transactionService;
            this.subscriberServices = subscriberServices;
        }

        [HttpGet("scr/{subscriberId}")]
        public ActionResult Script(int subscriberId = 0)
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/gfp.js";

            if (appSettings.Environment != "DEV")
            {
                scriptPath = $"{environment.ContentRootPath}/Scripts/gfp.min.js";
            }

            var bundleFile = new BundleFile(scriptPath);

            if (!string.IsNullOrWhiteSpace(bundleFile.Content))
            {
                var bundleContent = new BundleContent(subscriberId, bundleFile.Content, appSettings);

                return new JavascriptResult(bundleContent.Content);
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
                SessionKey = Guid.NewGuid().ToString("N"),
                Token = JWTService.GenerateJwtToken()
            });

            return Ok(jsonString);
        }



        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public void FingerPrint([FromBody] string data)
        {
            try
            {
                var scrypto = new Crypto(appSettings.EncryptPassword);

                var decryptedData = scrypto.Decrypt(data);

                var fingerPrint = JsonConvert.DeserializeObject<Models.FingerPrintModel>(decryptedData);

                if (fingerPrint != null && !string.IsNullOrWhiteSpace(fingerPrint.FingerPrint))
                {
                    FingerPrintEntity fingerPrintEntity = new FingerPrintEntity
                    {
                        FingerPrint = fingerPrint.FingerPrintCode,
                        Attributes = fingerPrint.FingerPrint,
                        SessionKey = fingerPrint.SessionKey,
                        SubscriberId = CheckExistSubscriber(fingerPrint.SubscriberId),
                        DeviceInfo = fingerPrint.Device,
                        DeviceCode = fingerPrint.DeviceCode,
                        CookieId = fingerPrint.CookieId
                    };
                    var kafkaServices = new KafkaServices(appSettings);
                    kafkaServices.SendFingerprintToKafka(fingerPrintEntity);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        [HttpPost("Transaction")]
        public bool Transaction([FromBody] TransactionModel data)
        {
            try
            {
                if (data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SessionKey) && data.SubscriberId != 0)
                {
                    var transaction = new TransactionModel
                    {
                        SubscriberId = CheckExistSubscriber(data.SubscriberId),
                        ClientIp = data.ClientIp,
                        LoginName = data.LoginName,
                        SessionKey = data.SessionKey,
                        TransTime = data.TransTime,
                        UserAgent = data.UserAgent,
                        Url = data.Url
                    };
                    var kafkaServices = new KafkaServices(appSettings);
                    kafkaServices.SendTransactionToKafka(transaction);
                    //transactionService.InsertTransaction(transaction);
                }

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }

            return false;
        }

        [HttpGet("subscriber/{subscriberId}")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<IEnumerable<string>> GetSubscriberInfo(int subscriberId)
        {
            var subscribers = subscriberServices.GetSubscriber(subscriberId);

            if (subscribers != null && subscribers.Any())
            {
                var domains = subscribers
                .Where(x => x.IsActived)
                .Select(x => x.DomainName);

                return Ok(domains);
            }

            return null;
        }

        [HttpGet("hc")]
        public void HealthCheck()
        {
            // Method intentionally left empty.
        }

        [HttpGet("getCookie/{cookie?}")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<string> GetCookie(string cookie)
        {
            var code = string.IsNullOrWhiteSpace(cookie) ? GenCookie() : cookie;

            var id = fingerPrintService.UpdateCookie(code);

            return Ok(new
            {
                Id = id,
                Code = code
            });
        }


        private string GenCookie()
        {
            Guid guiId = Guid.NewGuid();

            return guiId.ToString();
        }

        private int CheckExistSubscriber(int subscriberId)
        {
            var subscribers = subscriberServices.GetSubscriber(subscriberId);

            if (subscribers != null && subscribers.Any())
            {
                var subscriber = subscribers.FirstOrDefault(x => x.IsActived && x.IsMain);

                if (subscriber != null)
                {
                    return subscriber.SubscriberId;
                }
            }

            return -1;
        }
    }
}
