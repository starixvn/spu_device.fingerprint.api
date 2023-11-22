using Device.Fingerprint.WebAPI.Models;
using Fanex.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Device.Fingerprint.WebAPI.Controllers
{
    [Route("testing")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly ILogger logger;

        public TestingController(ILogger logger)
        {
            this.logger = logger;
        }

        // GET: TestingController
        [HttpGet("whoami")]
        public ActionResult<WhoamiModel> Index()
        {
            return new WhoamiModel
            {
                ServerName = HttpContext.Request.Headers["Server_Name"],
                ServerIP = HttpContext.Request.Headers["LOCAL_ADDR"],
                HTTP_CLIENT_IP = HttpContext.Request.Headers["HTTP_CLIENT_IP"],
                HTTP_X_FORWARDED_FOR = HttpContext.Request.Headers["HTTP_X_FORWARDED_FOR"],
                PROXY_CLIENT_IP = HttpContext.Request.Headers["Proxy-Client-IP"],
                REMOTE_ADDR = HttpContext.Request.Headers["REMOTE_ADDR"],
                ServerTime = DateTime.Now,
                WL_PROXY_CLIENT_IP = HttpContext.Request.Headers["WL-Proxy-Client-IP"],
                X_FORWARDED_FOR = HttpContext.Request.Headers["X-Forwarded-For"]
            };
        }

        [HttpGet("sentry-check")]
        public string SentryCheck()
        {
            logger.Info("Sentry is alive!");
            return "SentryCheck Done!";
        }
    }
}
