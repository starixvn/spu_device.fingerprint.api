using DeviceFingerprint.WebAPI.Models;
using DeviceFingerprint.WebAPI.Services;
using DeviceFingerprint.WebAPI.Ultilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DeviceFingerprint.WebAPI.Controllers
{
    [Route("api/fp")]
    [ApiController]    
    public class FingerPrintController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        private readonly IAppSettings appSettings;

        public FingerPrintController(IWebHostEnvironment Environment, IAppSettings appSettings)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
        }

        [HttpGet]
        public ActionResult<string> Script()
        {
            var bundleFile = new BundleFile($"{environment.ContentRootPath}/Scripts/gfp.min.js");

            var scriptContent = bundleFile.Content;

            if (!string.IsNullOrWhiteSpace(scriptContent))
            {
                var bundleContent = new BundleContent(scriptContent, appSettings);

                return bundleContent.Content;
            }

            return "Cannot get script.";
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public ActionResult<string> FingerPrint([FromBody] string data)
        {
            var scrypto = new Crypto(appSettings.EncryptPassword);
                        
            var decryptedData = scrypto.Decrypt(data);
            
            
            var fingerPrint = JsonConvert.DeserializeObject<FingerPrintModel>(decryptedData);
                       
            var attributes = JsonConvert.DeserializeObject<AttributeModel>(fingerPrint.Attributes);

            return decryptedData;
        }
        
        [HttpPost("Transaction")]
        public bool Transaction(Dictionary<string, string> data)
        {
            return true;
        }
    }
}
