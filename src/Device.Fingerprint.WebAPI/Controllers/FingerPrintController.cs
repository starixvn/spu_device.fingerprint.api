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
        private readonly ILogger logger;
        
        public FingerPrintController(
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            ISubscriberServices subscriberServices,            
            ILogger logger)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.subscriberServices = subscriberServices;            
            this.logger = logger;
        }

        [HttpGet("scr/{subscriberName}")]
        public ActionResult ScriptTemp(string subscriberName)
        {
            return new EmptyResult();
        }

        [HttpGet("s/ssk")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<string> GetSessionKey()
        {
            return Ok(Session.Generate());
        }

        [HttpGet("s/{subscriberName}")]
        [ETagFilter(200)]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Script(string subscriberName)
        {
            if (!string.IsNullOrWhiteSpace(subscriberName))
            {
                var scriptPath = $"{environment.ContentRootPath}/Scripts/fp.js";

                if (appSettings.Environment != "DEV")
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

        [HttpGet("s/ce/{version}")]
        public ActionResult CookieScript()
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/newCookie.js";

            if (appSettings.Environment != "DEV")
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
        
        [HttpGet("s/hl/{version}")]
        public ActionResult HeadlessBrowserScript()
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/headlessBrowser.js";

            if (appSettings.Environment != "DEV")
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

        [HttpGet("s/fp/{version}")]
        public ActionResult FingerPrintScript()
        {
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/gfp.js";

            if (appSettings.Environment != "DEV")
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

        [HttpGet("s/crt/{version}")]
        public ActionResult CryptScript(string version)
        {
            var scriptName = version.Contains("beta") ? "encrypt" : "md5";
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/{scriptName}.js";

            if (appSettings.Environment != "DEV")
            {
                scriptPath = $"{environment.ContentRootPath}/Scripts/libs/{scriptName}.min.js";
            }

            var bundleFile = new BundleFile(scriptPath);

            if (!string.IsNullOrWhiteSpace(bundleFile.Content))
            {
                return new JavascriptResult(bundleFile.Content);
            }

            return new EmptyResult();
        }

        [HttpGet("s/gjt")]
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

        [HttpGet("getClientIP")]
        public string GetClientIP()
        {
            var realIp = GetRealIp();
            var fakeIp = GetFakeIp();

            var result = "RealIP: " + realIp + "\nFakeIP: " + fakeIp;
            return result; 
        }

        private string GetFakeIp()
        {
            var proxyIP = GetXForWarded();

            if (!string.IsNullOrEmpty(proxyIP))
            {
                var ips = proxyIP.Split(new char[] { ',' });

                if (ips[0] != GetRealIp())
                {
                    return ips[0];
                }
            }

            return string.Empty;
        }

        private string GetHeaderIP(string param)
        {
            var headerVal = HttpContext.Request.Headers[param].ToString();
            if (!string.IsNullOrEmpty(headerVal))
            {
                return headerVal;
            }

            return string.Empty;
        }

        private string GetXForWarded()
        {
            var proxyIP = GetHeaderIP("X-Forwarded-For");

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Originating-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-Addr");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = HttpContext.Request.Host.ToString();
            }

            return proxyIP;
        }

        private string GetRealIp()
        {
            var proxyIP = GetHeaderIP("X-Actual-IP");

            if (string.IsNullOrEmpty(proxyIP))
            {
                var clientIps = GetXForWarded();

                if (!string.IsNullOrEmpty(clientIps))
                {
                    var ips = clientIps.Split(new char[] { ',' });

                    if (ips.Length > 1)
                    {
                        return ips[1];
                    }

                    return ips[0];
                }
                else
                {
                    return HttpContext.Request.Host.ToString();
                }
            }
            else
            {
                return proxyIP.Split(new char[] { ',' })[0];
            }
        }
    }
}
