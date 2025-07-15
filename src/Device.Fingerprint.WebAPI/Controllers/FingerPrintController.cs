using Device.Fingerprint.WebAPI.Services;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Core;
using Fanex.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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
    }
}
