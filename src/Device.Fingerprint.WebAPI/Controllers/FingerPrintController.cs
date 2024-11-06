using Device.Fingerprint.WebAPI.Models;
using Device.Fingerprint.WebAPI.Services;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Core;
using Fanex.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
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

        public FingerPrintController(
            IWebHostEnvironment Environment,
            IAppSettings appSettings)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
        }

        [HttpGet("s/f/p")]
        [ETagFilter]
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

        [HttpGet("s/c/r/t")]
        [ETagFilter]
        public ActionResult CryptScript()
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

        [HttpGet("hc")]
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

        [HttpGet("whoami")]
        public WhoamiModel WhoAmI()
        {
            var httpConnectionFeature = HttpContext.Features.Get<IHttpConnectionFeature>();
            var localIpAddress = httpConnectionFeature?.LocalIpAddress;

            return new WhoamiModel
            {
                ServerName = Environment.MachineName,
                ServerTime = DateTime.Now,
                LocalIP = localIpAddress.ToString(),
                ClientIP = GetHeaderIP("X-Remote-IP"),
                FakeIP = GetFakeIp(),
                RealIP = GetRealIp(),
                XFW = GetXForWarded(),
                ActualIP = GetHeaderIP("X-Actual-IP")
            };
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
