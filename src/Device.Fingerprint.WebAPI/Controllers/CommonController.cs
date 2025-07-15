using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Device.Fingerprint.WebAPI.Controllers
{
    [ApiController]
    public class CommonController(IAppSettings appSettings) : ControllerBase
    {
        private readonly IAppSettings appSettings = appSettings;

        [HttpGet("whoami")]
        public WhoamiModel Index()
        {
            var httpConnectionFeature = HttpContext.Features.Get<IHttpConnectionFeature>();
            var localIpAddress = httpConnectionFeature?.LocalIpAddress;

            return new WhoamiModel
            {
                Environment = appSettings.Environment,
                ServerName = Environment.MachineName,
                ServerTime = DateTime.Now,
                LocalIP = localIpAddress?.ToString()
            };
        }
    }
}
