using DeviceFingerprint.WebAPI.Models.FingerPrint;
using DeviceFingerprint.WebAPI.Ultilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using DeviceFingerprint.WebAPI.Services;

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

        [HttpGet("{subscriberId}")]
        public ActionResult<string> Script(int subscriberId)
        {
            var bundleFile = new BundleFile($"{environment.ContentRootPath}/Scripts/gfp.min.js");

            var scriptContent = bundleFile.Content;

            if(!string.IsNullOrWhiteSpace(scriptContent))
            {
                var bundleContent = new BundleContent(scriptContent, appSettings);

                return bundleContent.Content;
            }

            return null;
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public ActionResult<string> FingerPrint([FromBody] string data)
        {
            var scrypto = new Crypto(appSettings.EncryptPassword);
                        
            var decryptedData = scrypto.Decrypt(data);
            
            
            var fingerPrint = JsonConvert.DeserializeObject<FingerPrintModel>(decryptedData);

           var attributes = JsonConvert.DeserializeObject<AttributeModel>(fingerPrint.Attribute);

           return decryptedData;
        }

        [HttpGet("GetJWT")]
        public ActionResult<string> GetJWT()
        {
            var JWTService = new JWT(appSettings);
            return Ok(new { Token = JWTService.GenerateJwtToken(), Message = "Success" });
        }
        
        [HttpPost("Transaction")]
        public bool Transaction(Dictionary<string, string> data)
        {
            return true;
        }
    }
}
