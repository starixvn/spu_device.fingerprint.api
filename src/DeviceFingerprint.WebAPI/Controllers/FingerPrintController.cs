using Device.Fingerprint.WebAPI.Services;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Model;
using Fanex.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        private readonly IEasyNetQService easyNetQService;

        public FingerPrintController(
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            ISubscriberServices subscriberServices,
            IEasyNetQService easyNetQService)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.subscriberServices = subscriberServices;
            this.easyNetQService = easyNetQService;
        }

        [HttpGet("scr/{sessionKey}/{subscriberName}")]
        public ActionResult Script(string sessionKey, string subscriberName)
        {
            try
            {
                var scriptPath = $"{environment.ContentRootPath}/Scripts/gfp.js";

                if (appSettings.Environment != "DEV")
                {
                    scriptPath = $"{environment.ContentRootPath}/Scripts/gfp.min.js";
                }

                var bundleFile = new BundleFile(scriptPath);

                if (!string.IsNullOrWhiteSpace(bundleFile.Content))
                {
                    var subscribers = subscriberServices.GetSubscriberByName(subscriberName);

                    if (subscribers != null)
                    {
                        var bundleContent = new BundleContent(subscribers.SubscriberId, bundleFile.Content, appSettings, sessionKey);

                        return new JavascriptResult(bundleContent.Content);
                    }
                    else
                    {
                        LogService.Info("Subscriber " + subscriberName + " does not exists");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return null;
            }
        }

        [HttpGet("conf")]
        [ResponseCache(NoStore = true, Duration = 0)]
        public ActionResult<ScriptConfigModel> AppConfig()
        {
            try
            {
                var JWTService = new JWT(appSettings);

                string jsonString = JsonConvert.SerializeObject(new ScriptConfigModel
                {
                    EncryptedKey = appSettings.EncryptPassword,
                    Token = JWTService.GenerateJwtToken()
                });

                return Ok(jsonString);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Ok("");
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("fp")]
        public void FingerPrint([FromBody] string data)
        {
            try
            {
                var scrypto = new Crypto(appSettings.EncryptPassword);
                var decryptedData = scrypto.Decrypt(data);                                

                var fingerPrint = JsonConvert.DeserializeObject<Models.FingerPrintModel>(decryptedData);
                if (fingerPrint != null && !string.IsNullOrWhiteSpace(fingerPrint.FingerPrintCode))
                {
                    var clientTransactionModel = new ClientTransactionModel
                    {
                        FingerPrintCode = fingerPrint.FingerPrintCode,
                        SessionKey = fingerPrint.SessionKey,
                        SubscriberId = fingerPrint.SubscriberId,
                        CookieCode = fingerPrint.CookieCode,
                        Url = fingerPrint.Url,
                        TransTime = DateTime.Now
                    };

                    easyNetQService.PushData(clientTransactionModel);
                }
                else
                {
                    LogService.Info($"FingerPrint does not exists. ({data})");
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }
        }

        [HttpPost("transaction")]
        public void Transaction([FromBody] TransactionDTO data)
        {
            try
            {
                if (data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SessionKey) && !string.IsNullOrWhiteSpace(data.SubscriberName))
                {                    
                    var subscriber = subscriberServices.GetSubscriberByName(data.SubscriberName);

                    if (subscriber != null)
                    {
                        var urlAddress = new Uri(data.Url);

                        var transaction = new TransactionModel
                        {
                            SubscriberId = subscriber.SubscriberId,
                            ClientIp = data.ClientIp,
                            LoginName = data.LoginName,
                            SessionKey = data.SessionKey,
                            TransTime = DateTime.Now,
                            Url = urlAddress.GetLeftPart(UriPartial.Path),
                            Action = data.Action,
                            TransKey = data.TransKey,
                            CustId = data.CustId,
                            SubAccId = data.SubAccId
                        };

                        easyNetQService.PushData(transaction);
                    }
                    else
                    {
                        LogService.Info($"{data.SubscriberName} does not exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }
        }

        [HttpPost("fpa")]
        public void FpAttributes([FromBody] string data)
        {
            try
            {
                var scrypto = new Crypto(appSettings.EncryptPassword);
                var decryptedData = scrypto.Decrypt(data);

                var fpAttributes = JsonConvert.DeserializeObject<Models.FingerPrintAttributesModel>(decryptedData);
                if (fpAttributes != null && !string.IsNullOrWhiteSpace(fpAttributes.FingerPrintCode))
                {
                    var fpAttributesModel = new FingerprintAttributesModel
                    {
                        FingerPrintCode = fpAttributes.FingerPrintCode,
                        Attribute = fpAttributes.Attributes
                    };

                    easyNetQService.PushData(fpAttributesModel);
                }
                else
                {
                    LogService.Info($"FingerPrintCode cannot null. ({data})");
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }
        }

        [HttpPost("bi")]
        public void Browserinfo([FromBody] string data)
        {
            try
            {
                var crypto = new Crypto(appSettings.EncryptPassword);
                var decryptedData = crypto.Decrypt(data);

                var model = JsonConvert.DeserializeObject<Models.BrowserInfoJsonModel>(decryptedData);
                if (model != null && !string.IsNullOrWhiteSpace(model.SessionKey))
                {
                    var browserInfo = new BrowserInfo();
                    var (browserInfoModel, browserInfoAttributeModel) = browserInfo.GetBrowserInfo(HttpContext, model);

                    var browserInfoJson = JsonConvert.SerializeObject(browserInfoModel);
                    var browserInfoAttributeJson = JsonConvert.SerializeObject(browserInfoAttributeModel);
                    var browserInfoAttributeCode = crypto.GetMD5Hash(browserInfoAttributeJson);

                    var userAgentCode = crypto.GetMD5Hash(model.UserAgent);

                    var rawBrowserInfoModel = new BrowserInfoModel
                    {
                        SessionKey = model.SessionKey,
                        UserAgent = model.UserAgent,
                        UserAgentCode = userAgentCode,
                        HeadlessScore = model.HeadlessScore,
                        BrowserInfo = browserInfoJson,
                        browserInfoAttribute = browserInfoAttributeJson,
                        browserInfoAttributeCode = browserInfoAttributeCode
                    };

                    easyNetQService.PushData(rawBrowserInfoModel);
                }
                else
                {
                    LogService.Info($"SessionKey does not exists. ({data})");
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }
        }

        [HttpGet("getsubscriber/{subscriberName}")]
        public ActionResult<SubscriberModel> GetSubscriberByName(string subscriberName)
        {
            var subscriber = subscriberServices.GetSubscriberByName(subscriberName);
            if (subscriber != null)
            {
                return Ok(subscriber);
            }
            else
            {
                LogService.Info("Subscriber " + subscriberName + " does not exists. API get-subscriber-by-name");

                return Ok(null);
            }
        }

        [HttpGet("hc")]
        public void HealthCheck()
        {
            // Method intentionally left empty.            
        }
    }
}