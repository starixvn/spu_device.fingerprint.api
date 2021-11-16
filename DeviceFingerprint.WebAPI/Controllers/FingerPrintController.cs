using DeviceFingerprint.WebAPI.Models;
using DeviceFingerprint.WebAPI.Services;
using DeviceFingerprint.WebAPI.Ultilities;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using Fps.Device.FingerPrint.Features.FingerPrint.Services;
using Fps.Device.FingerPrint.Features.Subscriber.Services;
using Fps.Device.FingerPrint.Features.Transaction.Services;
using FPS.Device.FingerPrint.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;

namespace DeviceFingerprint.WebAPI.Controllers
{
    [Route("api/fp")]
    [ApiController]
    public class FingerPrintController : ControllerBase
    {
        private readonly ISentryClient sentryClient;
        private readonly IWebHostEnvironment environment;
        private readonly IAppSettings appSettings;
        private readonly IFingerPrintService fingerPrintService;
        private readonly ITransactionService transactionService;
        private readonly ISubscriberServices subscriberServices;

        public FingerPrintController(ISentryClient sentryClient, 
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            IFingerPrintService fingerPrintService,
            ITransactionService transactionService,
            ISubscriberServices subscriberServices)
        {
            this.sentryClient = sentryClient;
            this.environment = Environment;
            this.appSettings = appSettings;
            this.fingerPrintService = fingerPrintService;
            this.transactionService = transactionService;
            this.subscriberServices = subscriberServices;
        }

        [HttpGet("{subscriberId}")]
        public ActionResult<string> Script(int subscriberId = 0)
        {
            var bundleFile = new BundleFile($"{environment.ContentRootPath}/Scripts/gfp.min.js");                        

            if (!string.IsNullOrWhiteSpace(bundleFile.Content))
            {
                var subscriber = subscriberServices.GetSubscriber(subscriberId);

                if(subscriber != null && subscriber.SubscriberId != 0 && subscriber.DCSStatus)
                {
                    subscriberId = subscriber.SubscriberId;
                }
                else
                {
                    subscriberId = -1;
                }

                var bundleContent = new BundleContent(subscriberId, bundleFile.Content, appSettings);

                return bundleContent.Content;
            }

            return "Cannot get script.";
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        public ActionResult<string> FingerPrint([FromBody] string data)
        {
            try
            {
                var scrypto = new Crypto(appSettings.EncryptPassword);

                var decryptedData = scrypto.Decrypt(data);

                var fingerPrint = JsonConvert.DeserializeObject<Models.FingerPrintModel>(decryptedData);

                if (fingerPrint != null && !string.IsNullOrWhiteSpace(fingerPrint.FingerPrint))
                {
                    int subscriberId = string.IsNullOrWhiteSpace(fingerPrint.SubscriberId) ? 0 : int.Parse(fingerPrint.SubscriberId);
                    fingerPrintService.InsertFingerPrint(new FingerPrintEntity
                    {
                        FingerPrint = fingerPrint.FingerPrint,
                        Attributes = fingerPrint.Attributes,
                        SessionKey = fingerPrint.Key,
                        SubscriberId = subscriberId
                    });
                }

                return decryptedData;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }

            return string.Empty;
        }

        [HttpPost("Transaction")]
        public bool Transaction([FromBody] TransactionModel data)
        {
            try
            {
                if (data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SessionKey) && data.SubscriberId != 0)
                {
                    transactionService.InsertTransaction(data);
                }               

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }           

            return false;
        }
    }
}
