using DeviceFingerprint.WebAPI.Models;
using DeviceFingerprint.WebAPI.Services;
using DeviceFingerprint.WebAPI.Ultilities;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using Fps.Device.FingerPrint.Features.FingerPrint.Services;
using Fps.Device.FingerPrint.Features.Transaction.Services;
using FPS.Device.FingerPrint.Model;
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
        private readonly IFingerPrintService fingerPrintService;
        private readonly ITransactionService transactionService;

        public FingerPrintController(IWebHostEnvironment Environment,
            IAppSettings appSettings,
            IFingerPrintService fingerPrintService,
            ITransactionService transactionService)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.fingerPrintService = fingerPrintService;
            this.transactionService = transactionService;
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
            
            
            var fingerPrint = JsonConvert.DeserializeObject<Models.FingerPrintModel>(decryptedData);
                       
            var attributes = JsonConvert.DeserializeObject<AttributeModel>(fingerPrint.Attributes);

            if (fingerPrint != null && !string.IsNullOrWhiteSpace(fingerPrint.DI))
            {
                fingerPrintService.InsertFingerPrint(new FingerPrintEntity
                {
                    DeviceIdentify = fingerPrint.DI,
                    Attributes = fingerPrint.Attributes,
                    RoutingKey = fingerPrint.Key
                });
            }
            return decryptedData;
        }
        
        [HttpPost("Transaction")]
        public bool Transaction([FromBody] TransactionModel data)
        {
           if(data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SessionKey) && data.SubscriberId != 0)
            {
                transactionService.InsertTransaction(data);

                return true;
            }

            return false;
        }
    }
}
