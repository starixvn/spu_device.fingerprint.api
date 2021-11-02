using DeviceFingerprint.WebAPI.Models;
using DeviceFingerprint.WebAPI.Ultilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;

        public TokenController(IWebHostEnvironment Environment)
        {
            this.environment = Environment;
        }

        [HttpGet]
        public ActionResult<TokenResultModel> Get(int subscriberId)
        {
            var bundleFile = new BundleFile($"{environment.ContentRootPath}/Scripts/gfp.min.js");

            var scriptContent = bundleFile.Content;

            if(!string.IsNullOrWhiteSpace(scriptContent))
            {
                var bundleContent = new BundleContent(scriptContent);

                return new TokenResultModel
                {
                    JsScript = bundleContent.Content
                };
            }

            return null;
        }
    }
}
