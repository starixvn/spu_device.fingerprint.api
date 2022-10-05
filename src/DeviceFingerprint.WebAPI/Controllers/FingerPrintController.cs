using Device.Fingerprint.WebAPI.Services;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Features.ClientTransaction.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sentry;
using Sentry.Protocol;
using System;

namespace Device.Fingerprint.WebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class FingerPrintController : ControllerBase
    {

        private readonly IWebHostEnvironment environment;
        private readonly IAppSettings appSettings;
        private readonly ISubscriberServices subscriberServices;
        private readonly IClientTransactionService clientTransactionService;
        private readonly IEasyNetQService easyNetQService;
        private readonly Crypto crypto;
        private readonly IRedisService redisService;
        private readonly ISentryClient sentryClient;

        public FingerPrintController(
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            ISubscriberServices subscriberServices,
            IEasyNetQService easyNetQService,
            IRedisService redisService,
            ISentryClient sentryClient,
            IClientTransactionService clientTransactionService)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.subscriberServices = subscriberServices;
            this.easyNetQService = easyNetQService;
            crypto = new Crypto(appSettings.CryptoPassword, appSettings.CryptoIV, appSettings.CryptoSalt);
            this.redisService = redisService;
            this.sentryClient = sentryClient;
            this.clientTransactionService = clientTransactionService;
        }

        [HttpGet("scr/{sessionKey}/{subscriberName}")]
        public ActionResult Script(string sessionKey, string subscriberName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(sessionKey) && !string.IsNullOrWhiteSpace(subscriberName) && Validation.SessionKey(sessionKey))
                {
                    var scriptPath = $"{environment.ContentRootPath}/Scripts/fp.js";

                    if (appSettings.Environment == "PROD")
                    {
                        scriptPath = $"{environment.ContentRootPath}/Scripts/fp.min.js";
                    }

                    var bundleFile = new BundleFile(scriptPath);

                    if (!string.IsNullOrWhiteSpace(bundleFile.Content))
                    {
                        var subscribers = subscriberServices.GetSubscriberByName(subscriberName);

                        if (subscribers != null)
                        {
                            var bundleContent = new BundleContent(subscribers.SubscriberId, bundleFile.Content, appSettings, sessionKey);

                            return new JavascriptResult(bundleContent.Content);
                        }
                        else
                        {
                            sentryClient.CaptureMessage("Subscriber " + subscriberName + " does not exists", SentryLevel.Info);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }

            return new EmptyResult();
        }

        [HttpGet("scr/cdn/gfp")]
        public ActionResult CDNScript()
        {
            try
            {
                var scriptPath = $"{environment.ContentRootPath}/Scripts/gfp.js";

                if (appSettings.Environment == "PROD")
                {
                    scriptPath = $"{environment.ContentRootPath}/Scripts/gfp.min.js";
                }

                var bundleFile = new BundleFile(scriptPath);

                if (!string.IsNullOrWhiteSpace(bundleFile.Content))
                {

                    return new JavascriptResult(bundleFile.Content);

                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }

            return new EmptyResult();
        }

        [HttpGet("conf/{sessionKey}")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<ScriptConfigModel> AppConfig(string sessionKey)
        {
            try
            {
                if (Validation.SessionKey(sessionKey))
                {
                    var JWTService = new JWT(appSettings);

                    string jsonString = JsonConvert.SerializeObject(new ScriptConfigModel
                    {
                        EncryptedKey = appSettings.CryptoPassword,
                        Token = JWTService.GenerateJwtToken(),
                        IV = appSettings.CryptoIV,
                        Salt = appSettings.CryptoSalt
                    });

                    return Ok(jsonString);
                }



            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }

            return Ok(string.Empty);
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [HttpPost("fp")]
        public void FingerPrint([FromBody] string data)
        {
            try
            {                
                var decryptedData = crypto.Decrypt(data);

                var fingerPrint = JsonConvert.DeserializeObject<Models.FingerPrintModel>(decryptedData);
                if (fingerPrint != null && !string.IsNullOrWhiteSpace(fingerPrint.FingerPrintCode))
                {
                    if (!Validation.DuplicateSessionKey(redisService, fingerPrint.SessionKey, fingerPrint.SubscriberId))
                    {
                        var defenderAttributesService = new DefenderAttributesService();

                        var clientTransactionModel = new ClientTransactionModel
                        {
                            FingerPrintCode = fingerPrint.FingerPrintCode,
                            SessionKey = fingerPrint.SessionKey,
                            SubscriberId = fingerPrint.SubscriberId,
                            CookieCode = fingerPrint.CookieCode,
                            Url = fingerPrint.Url,
                            AddonCode = defenderAttributesService.GenerateAddonCode(fingerPrint.DefenderAttributes),
                            TransTime = DateTime.Now
                        };

                        easyNetQService.PushData(clientTransactionModel);
                    }
                }
                else
                {
                    sentryClient.CaptureMessage($"FingerPrint does not exists. ({data})", SentryLevel.Info);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        [HttpPost("ctl")]
        public void ClientTransLog([FromBody] ClientTransactionLogsModel data)
        {
            try
            {
                data.TransTime = DateTime.Now;

                easyNetQService.PushData(data);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        [HttpPost("cle")]
        public void ClientTransException([FromBody] string data)
        {
            try
            {
                if(!string.IsNullOrWhiteSpace(data))
                {
                    var logsData = JsonConvert.DeserializeObject<ClientTransactionExceptionModel>(data);

                    logsData.UserAgent = Request.Headers["User-Agent"];
                    clientTransactionService.InsertClientTransactionException(logsData.Function, logsData.Exception, logsData.UserAgent);
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
                    var subscriber = subscriberServices.GetSubscriberByName(data.SubscriberName);

                    if (subscriber != null)
                    {
                        var urlAddress = new Uri(data.Url);

                        var userAgentCode = crypto.GetMD5Hash(data.UserAgent);

                        var transaction = new TransactionModel
                        {
                            SubscriberId = subscriber.SubscriberId,
                            ClientIp = data.ClientIp,
                            LoginName = data.LoginName,
                            SessionKey = data.SessionKey,
                            UserAgent = data.UserAgent,
                            UserAgentCode = userAgentCode,
                            TransTime = DateTime.Now,
                            Url = urlAddress.GetLeftPart(UriPartial.Path),
                            Action = data.Action,
                            TransKey = data.TransKey,
                            CustId = data.CustId,
                            SubAccId = data.SubAccId
                        };

                        easyNetQService.PushData(transaction);
                    }
                    else
                    {
                        sentryClient.CaptureMessage($"{data.SubscriberName} does not exists.", SentryLevel.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [HttpPost("fpa")]        
        public void FpAttributes([FromBody] string data)
        {
            try
            {
                var decryptedData = crypto.Decrypt(data);

                var fpAttributes = JsonConvert.DeserializeObject<Models.FingerPrintAttributesModel>(decryptedData);
                if (fpAttributes != null && !string.IsNullOrWhiteSpace(fpAttributes.FingerPrintCode))
                {
                    var fpAttributesModel = new FingerprintAttributesModel
                    {
                        FingerPrintCode = fpAttributes.FingerPrintCode,
                        Attribute = fpAttributes.Attributes
                    };

                    easyNetQService.PushData(fpAttributesModel);
                }
                else
                {
                    sentryClient.CaptureMessage($"FingerPrintCode cannot null. ({data})", SentryLevel.Info);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [HttpPost("bi")]
        public void Browserinfo([FromBody] string data)
        {
            try
            {
                var decryptedData = crypto.Decrypt(data);

                var model = JsonConvert.DeserializeObject<Models.BrowserInfoJsonModel>(decryptedData);
                if (model != null && !string.IsNullOrWhiteSpace(model.SessionKey))
                {

                    var browserInfo = new BrowserInfo();
                    var (browserInfoModel, browserInfoAttributeModel) = browserInfo.GetBrowserInfo(HttpContext, model, sentryClient);

                    var browserInfoJson = JsonConvert.SerializeObject(browserInfoModel);
                    var browserInfoAttributeJson = JsonConvert.SerializeObject(browserInfoAttributeModel);
                    var browserInfoAttributeCode = crypto.GetMD5Hash(browserInfoAttributeJson);

                    var rawBrowserInfoModel = new BrowserInfoModel
                    {
                        SessionKey = model.SessionKey,
                        HeadlessScore = model.HeadlessScore,
                        BrowserInfo = browserInfoJson,
                        BrowserInfoAttribute = browserInfoAttributeJson,
                        BrowserInfoAttributeCode = browserInfoAttributeCode
                    };

                    easyNetQService.PushData(rawBrowserInfoModel);
                }
                else
                {
                    sentryClient.CaptureMessage($"SessionKey does not exists. ({data})", SentryLevel.Info);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        [HttpGet("getsubscriber/{subscriberName}")]
        public ActionResult<SubscriberModel> GetSubscriberByName(string subscriberName)
        {
            var subscriber = subscriberServices.GetSubscriberByName(subscriberName);
            if (subscriber != null)
            {
                return Ok(subscriber);
            }
            else
            {
               /// sentryClient.CaptureMessage("Subscriber " + subscriberName + " does not exists. API get-subscriber-by-name", SentryLevel.Info);

                return Ok(null);
            }
        }

        [HttpGet("hc")]
        public void HealthCheck()
        {
            // Method intentionally left empty.            
        }

        [HttpGet("sessionkey")]
        public ActionResult<string> SessionKey()
        {
            var guid = Guid.NewGuid().ToString("N");
            var expiredTime = DateTime.UtcNow.AddMinutes(1);

            return string.Format("{0}{1}{2}{3}{4}"
                , Hashing.GetRandomHexNumber(8).ToLower()
                , expiredTime.ToString("yyyyMMdd")
                , guid
                , expiredTime.ToString("hhmmss")
                , Hashing.GetRandomHexNumber(8).ToLower());
        }

        [HttpGet("ValidateSessionKey")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<bool> ValidateSessionKey(string sessionKey)
        {
            return Validation.SessionKey(sessionKey);
        }

        [HttpPost("PostRedis")]
        public void PostRedis(string key, string value)
        {
            redisService.RedisDatabase.AddAsync(key, value);
        }

        [HttpGet("GetRedis")]
        public ActionResult<string> GetRedis(string key)
        {

            if (redisService.RedisDatabase.ExistsAsync(key).Result)
            {
                return redisService.RedisDatabase.GetAsync<string>(key).Result;
            }

            return "No values";
        }

        [HttpGet("DecryptData")]
        public ActionResult<string> DecryptData(string key)
        {
            return crypto.Decrypt(key);
        }
    }
}
