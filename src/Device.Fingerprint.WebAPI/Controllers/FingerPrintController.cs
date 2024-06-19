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

        public FingerPrintController(
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            ISubscriberServices subscriberServices)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.subscriberServices = subscriberServices;
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
                        LogService.Info("Subscriber " + subscriberName + " does not exists");
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
            var scriptPath = $"{environment.ContentRootPath}/Scripts/libs/encrypt.js";
            var storageScriptPath = $"{environment.ContentRootPath}/Scripts/libs/newCookie.js";

            if (appSettings.Environment != "DEV")
            {
                scriptPath = $"{environment.ContentRootPath}/Scripts/libs/encrypt.min.js";
                storageScriptPath = $"{environment.ContentRootPath}/Scripts/libs/newCookie.min.js";
            }

            var bundleFile = new BundleFile(scriptPath);
            var storageScriptFile = new BundleFile(storageScriptPath);

            if (!string.IsNullOrWhiteSpace(bundleFile.Content) || !string.IsNullOrWhiteSpace(storageScriptFile.Content))
            {
                var combinedContent = $"{bundleFile.Content}\n{storageScriptFile.Content}";
                return new JavascriptResult(combinedContent);
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

        [HttpGet("s/tok")]
        [HttpPost("s/tok")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult GetToken()
        {
            var JWTService = new JWTService(appSettings);

            return Ok(new
            {
                tk = JWTService.GenerateJwtToken(),
                ss = Session.Generate()
            });
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
            // LogService.Info("I'm good.");
        }

        [HttpGet("hc-sentry")]
        public string HealthCheckSentry()
        {
            LogService.Info("I'm good.");
            return "I'm good.";
        }


        [HttpGet]
        [Route("getToken")]
        public string GetToken(string env = "local")
        {
            if (env == "prod")
            {
                return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6Ii8vbG9jYWxob3N0OjUwMDcvYXBpIiwibmJmIjoxNzA4NTA5MjE5LCJleHAiOjI1MzQwMjI3NTYwMCwiaWF0IjoxNzA4NTA5MjE5LCJpc3MiOiIvL2xvY2FsaG9zdDo1MDA3L2FwaSJ9.0ot1gLv11INDOPcMxBPXoP5WWWT0xUhS2d1GQPStVvY";
            }

            return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6Ii8vbG9jYWxob3N0OjUwMDcvYXBpIiwibmJmIjoxNzA4NTA3NTM0LCJleHAiOjI1MzQwMjI3NTYwMCwiaWF0IjoxNzA4NTA3NTM0LCJpc3MiOiIvL2xvY2FsaG9zdDo1MDA3L2FwaSJ9.fiE102TDhb14_rG4NvgGXRk_nwiEKZxGVT9Iq0JWycs";
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
