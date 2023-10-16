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
        //private readonly IRedisService redisService;
        private readonly ILogger logger;
        
        public FingerPrintController(
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            ISubscriberServices subscriberServices,
            //IRedisService redisService,
            ILogger logger)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.subscriberServices = subscriberServices;
            //this.redisService = redisService;
            this.logger = logger;
        }

        [HttpGet("scr/{subscriberName}")]
        [ETagFilter(200)]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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
                        var referer = HttpContext.Request.GetTypedHeaders().Referer;

                        var refererUrl = referer != null ? referer.ToString() : HttpContext.Request.GetDisplayUrl();
                        var scheme = new Uri(refererUrl).Scheme;
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

        [HttpGet("scr/{version}/ce")]
        //[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "version" })]            
        public ActionResult CookieScript(string version)
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
        
        [HttpGet("scr/{version}/hl")]
        //[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "version" })]
        public ActionResult HeadlessBrowserScript(string version)
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

        [HttpGet("scr/{version}/bi")]
        //[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "version" })]
        public ActionResult BrowserInfoScript(string version)
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

        [HttpGet("scr/{version}/fp")]
       // [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "version" })]
        public ActionResult FingerPrintScript(string version)
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

        [HttpGet("scr/{version}/crt")]
        //[ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new string[] { "version" })]
        public ActionResult CryptScript(string version)
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/md5.js";

            if (appSettings.Environment.ToUpper() == "PROD" || appSettings.Environment.ToUpper() == "PRO")
            {
                scriptPath = $"{environment.ContentRootPath}/Scripts/libs/md5.min.js";
            }

            var bundleFile = new BundleFile(scriptPath);

            if (!string.IsNullOrWhiteSpace(bundleFile.Content))
            {
                return new JavascriptResult(bundleFile.Content);
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

        [HttpGet("getsubscriber/{subscriberName}")]
        public ActionResult<SubscriberModel> GetSubscriberByName(string subscriberName)
        {
            var subscriber = subscriberServices.GetSubscriberByName(subscriberName);
            if (subscriber != null)
            {
                return Ok(subscriber);
            }

            return Ok(null);
        }

        [HttpGet("hc")]
        [ETagFilter(200)]
        public void HealthCheck()
        {
            // Method intentionally left empty.
            // 
            logger.Info("I'm good.");
        }        
    }
}
