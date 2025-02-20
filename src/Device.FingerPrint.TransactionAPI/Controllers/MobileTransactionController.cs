using Device.FingerPrint.Model;
using Device.FingerPrint.Queue;
using Device.FingerPrint.TransactionAPI.Model;
using Device.FingerPrint.TransactionAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Fanex.Logging;

namespace Device.FingerPrint.TransactionAPI.Controllers
{
    [ApiController]
    public class MobileTransactionController : ControllerBase
    {
        private readonly IAppSettings appSettings;
        private readonly Fanex.Logging.ILogger logger;
        private readonly ISubscriberService subscriberService;
        private readonly IMassTransitProducer massTransitProducer;

        public MobileTransactionController(
            IAppSettings appSettings,
            Fanex.Logging.ILogger logger,
            ISubscriberService subscriberService,
            IMassTransitProducer massTransitProducer)
        {
            this.appSettings = appSettings;
            this.logger = logger;
            this.subscriberService = subscriberService;
            this.massTransitProducer = massTransitProducer;
        }

        [HttpPost("FPS/InsertMobileTransaction")]
        public IActionResult InsertTransactionAsync(MobileTransactionModel transaction)
        {
            if (transaction == null)
            {
                return BadRequest("Invalid transaction data.");
            }

            transaction.IP = HttpContext.Request.Headers["Fps-Forwarded-For"].ToString();

            PushToQueueAsync(transaction);

            return Ok();
        }

        private MobileTransactionModel TransformTransactionRequest(MobileTransactionModel transaction)
        {
            //byte[] base64DecodedBytes = Convert.FromBase64String(request);

            //string decodedString = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);

            //var transaction = Newtonsoft.Json.JsonConvert.DeserializeObject<MobileTransactionModel>(decodedString);

            //transaction.IP = HttpContext.Request.Headers["Fps-Forwarded-For"].ToString();

            return transaction;
        }

        private void PushToQueueAsync(MobileTransactionModel transactionRequest)
        {
            var message = new MessageQueue
            {
                Account = transactionRequest.Account,
                Action = transactionRequest.Action,
                ActionResult = transactionRequest.ActionResult,
                Subscriber = transactionRequest.Subscriber,
                IP = transactionRequest.IP,
                TransDate = transactionRequest.TransDate,
                SubscriberDate = DateTime.Now,
                Mobile = new MobileModel
                {
                    MachineCode = !string.IsNullOrEmpty(transactionRequest.AndroidCode) ? transactionRequest.AndroidCode : transactionRequest.MachineCode,
                    Brand = transactionRequest.Brand,
                    Country = transactionRequest.Country,
                    CustomCode = transactionRequest.CustomCode,
                    IsEmulator = transactionRequest.IsEmulator,
                    IsMobileAppTrans = true,
                    Manufacturer = transactionRequest.Manufacturer,
                    MediaDRMCode = transactionRequest.MediaDRMCode,
                    ModelName = transactionRequest.ModelName,
                    OS = transactionRequest.OS,
                    SdkVersion = transactionRequest.SdkVersion,
                    SystemLanguage = transactionRequest.SystemLanguage,
                    Timezone = transactionRequest.Timezone
                }
            };                        

            var queueAddress = subscriberService.IsExtraSubscriber(transactionRequest.Subscriber) ? appSettings.ExtraQueueName : appSettings.MobileQueueName;

            massTransitProducer.Send(message, queueAddress);            
        }
    }
}
