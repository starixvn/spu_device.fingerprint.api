using Device.Fingerprint.WebAPI.Services;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Core;
using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Model;
using Fanex.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private readonly IRedisService redisService;
        private readonly ILogger logger;        

        public FingerPrintController(
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            ISubscriberServices subscriberServices,
            IRedisService redisService,
            ILogger logger)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.subscriberServices = subscriberServices;
            this.redisService = redisService;
            this.logger = logger;
        }

        [HttpGet("scr/{subscriberName}")]
        [ETagFilter(200)]
        public ActionResult Script(string subscriberName)
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
                        var refererUrl = HttpContext.Request.GetTypedHeaders().Referer.ToString();
                        var scheme = new Uri(refererUrl).Scheme;

                        logger.Info("scheme:" + scheme + "- refererUrl:" + refererUrl);

                        var bundleContent = new BundleContent(subscribers.SubscriberId, bundleFile.Content, appSettings, sessionKey, scheme);

                        return new JavascriptResult(bundleContent.Content);
                    }
                    else
                    {
                        logger.Info("Subscriber " + subscriberName + " does not exists");
                    }
                }
            }

            return new EmptyResult();
        }

        [HttpGet("scr/cdn/gfp")]
        public ActionResult CDNScript()
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

            return new EmptyResult();
        }

        [HttpGet("scr/cdn/ce")]
        [ETagFilter(200)]
        public ActionResult CookieScript()
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/newCookie.js";

            if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
            {
                scriptPath = $"{environment.ContentRootPath}/Scripts/libs/newCookie.min.js";
            }

            var bundleFile = new BundleFile(scriptPath);

            if (!string.IsNullOrWhiteSpace(bundleFile.Content))
            {
                return new JavascriptResult(bundleFile.Content);
            }

            return new EmptyResult();
        }

        [HttpGet("scr/cdn/ce1")]
        [ETagFilter(200)]
        public ActionResult Cookie1Script()
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/cookie1.js";

            if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
            {
                scriptPath = $"{environment.ContentRootPath}/Scripts/libs/cookie1.min.js";
            }

            var bundleFile = new BundleFile(scriptPath);

            if (!string.IsNullOrWhiteSpace(bundleFile.Content))
            {
                return new JavascriptResult(bundleFile.Content);
            }

            return new EmptyResult();
        }

        [HttpGet("scr/cdn/hl")]
        [ETagFilter(200)]
        public ActionResult HeadlessBrowserScript()
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

            return new EmptyResult();
        }

        [HttpGet("scr/cdn/bi")]
        [ETagFilter(200)]
        public ActionResult BrowserInfoScript()
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

            return new EmptyResult();
        }

        [HttpGet("scr/cdn/fp")]
        [ETagFilter(200)]
        public ActionResult FingerPrintScript()
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

            return new EmptyResult();
        }

        [HttpGet("scr/cdn/crt")]
        [ETagFilter(200)]
        public ActionResult CryptScript()
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/crypt.js";

            if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
            {
                scriptPath = $"{environment.ContentRootPath}/Scripts/libs/crypt.min.js";
            }

            var bundleFile = new BundleFile(scriptPath);

            if (!string.IsNullOrWhiteSpace(bundleFile.Content))
            {
                var newContent = bundleFile.Content
                    .Replace("__keyIV__", appSettings.CryptoIV)
                    .Replace("__keySalt__", appSettings.CryptoSalt)
                    .Replace("__keyPass__", appSettings.CryptoPassword);

                return new JavascriptResult(newContent);
            }

            return new EmptyResult();
        }

        [HttpGet("scr/gjt")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<string> GetJWT()
        {
            var JWTService = new JWTService(appSettings);
            return Ok(JWTService.GenerateJwtToken());
        }

        [HttpGet("conf/{sessionKey}")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<ScriptConfigModel> AppConfig(string sessionKey)
        {
            if (Validation.SessionKey(sessionKey))
            {
                var JWTService = new JWTService(appSettings);

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

            return Ok(string.Empty);
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
                logger.Info("Subscriber " + subscriberName + " does not exists. API get-subscriber-by-name");

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
