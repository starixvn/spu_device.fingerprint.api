using Device.Fingerprint.WebAPI.Services;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Features.ClientTransaction.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly IRedisService redisService;
        private readonly ISentryClient sentryClient;

        private string etagValue;

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
            this.redisService = redisService;
            this.sentryClient = sentryClient;
            this.clientTransactionService = clientTransactionService;

            etagValue = etagValue ?? Guid.NewGuid().ToString("N");
        }

        [HttpGet("scr/{subscriberName}")]
        [ETagFilter(200)]
        public ActionResult Script(string subscriberName)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(subscriberName))
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
                            var sessionKey = Session.Generate();

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

                if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
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

        [HttpGet("scr/cdn/ce")]   
        [ETagFilter(200)]
        public ActionResult CookieScript()
        {
            try
            {
                var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/cookie.js";

                if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
                {
                    scriptPath = $"{environment.ContentRootPath}/Scripts/libs/cookie.min.js";
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

        [HttpGet("scr/cdn/hl")]
        [ETagFilter(200)]
        public ActionResult HeadlessBrowserScript()
        {
            try
            {

                var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/headlessBrowser.js";

                if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
                {
                    scriptPath = $"{environment.ContentRootPath}/Scripts/libs/headlessBrowser.min.js";
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

        [HttpGet("scr/cdn/bi")]
        [ETagFilter(200)]
        public ActionResult BrowserInfoScript()
        {
            try
            {
                var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/browserInfo.js";

                if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
                {
                    scriptPath = $"{environment.ContentRootPath}/Scripts/libs/browserInfo.min.js";
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

        [HttpGet("scr/cdn/fp")]
        [ETagFilter(200)]
        public ActionResult FingerPrintScript()
        {
            try
            {
                var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/gfp.js";

                if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
                {
                    scriptPath = $"{environment.ContentRootPath}/Scripts/libs/gfp.min.js";
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
                        Salt = appSettings.CryptoSalt,
                        Accept = HttpContext.Request.Headers["Accept"],
                        Encoding = HttpContext.Request.Headers["Accept-Encoding"],
                        HeaderKeys = String.Join(" ", HttpContext.Request.Headers.Keys)
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

        [HttpPost("ctl")]
        public void ClientTransLog([FromBody] ClientTransactionLogsModel data)
        {
            try
            {
                if (!appSettings.IgnoreClientTransLogs)
                {
                    data.TransTime = DateTime.Now;

                    easyNetQService.PushData(data);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        [HttpPost("cle")]
        public void ClientTransException([FromBody] ClientTransactionExceptionModel logsData)
        {
            try
            {
                logsData.UserAgent = Request.Headers["User-Agent"];

                clientTransactionService.InsertClientTransactionException(logsData.Function, logsData.Exception, logsData.UserAgent);

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
    }
}
