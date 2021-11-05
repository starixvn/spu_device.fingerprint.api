using DeviceFingerprint.WebAPI.Models;
using DeviceFingerprint.WebAPI.Models.FingerPrint;
using DeviceFingerprint.WebAPI.Ultilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeviceFingerprint.WebAPI.Controllers
{
    [Route("api/fp")]
    [ApiController]    
    public class FingerPrintController : ControllerBase
    {
        private readonly IWebHostEnvironment environment;
        private readonly IAppSettings appSettings;
        private readonly IConfiguration configuration;

        public FingerPrintController(IWebHostEnvironment Environment, IAppSettings appSettings, IConfiguration configuration)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.configuration = configuration;
        }

        [HttpGet("GetToken")]
        public ActionResult<string> GetToken(int subscriberId)
        {
            return Tools.GenerateRandomKey(subscriberId);
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("GetScript")]
        public ActionResult<string> Script()
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
        public ActionResult<string> GetJWT(int subscriberId)
        {
            var tokenString = GenerateJwtToken(subscriberId);
            return Ok(new { Token = tokenString, Message = "Success" });
        }
        private string GenerateJwtToken(int subscriberId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["AppConfig:EncryptPassword"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", subscriberId.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(1),
                Issuer = configuration["AppConfig:ApiUrl"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        [HttpPost("Transaction")]
        public bool Transaction(Dictionary<string, string> data)
        {
            return true;
        }
    }
}
