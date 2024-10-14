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
        public IActionResult InsertTransactionAsync(MobileTransactionRequest request)
        {
            var transaction = TransformTransactionRequest(request);

            _ = PushToQueueAsync(transaction);

            return Ok();
        }

        private MobileTransactionModel TransformTransactionRequest(MobileTransactionRequest request)
        {
            byte[] base64DecodedBytes = Convert.FromBase64String(request.Transaction);

            string decodedString = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);

            var transaction = Newtonsoft.Json.JsonConvert.DeserializeObject<MobileTransactionModel>(decodedString);

            transaction.IP = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            return transaction;
        }

        private async Task PushToQueueAsync(MobileTransactionModel transactionRequest)
        {
            var message = new MessageQueue
            {
                Account = transactionRequest.Account,
                Action = transactionRequest.Action,
                ActionResult = transactionRequest.ActionResult,
                Subscriber = transactionRequest.Subscriber,
                IP = transactionRequest.IP,
                TransDate = DateTime.Now,
                SubscriberDate = DateTime.Now,
                Mobile = new MobileModel
                {
                    AndroidCode = transactionRequest.AndroidCode,
                    Brand = transactionRequest.Brand,
                    Country = transactionRequest.Country,
                    CustomCode = transactionRequest.CustomCode,
                    IsEmulator = transactionRequest.IsEmulator,
                    IsMobileAppTrans = transactionRequest.IsMobileApp,
                    UserAgent = transactionRequest.UserAgent,
                    Manufacturer = transactionRequest.Manufacturer,
                    MediaDRMCode = transactionRequest.MediaDRMCode,
                    ModelName = transactionRequest.ModelName,
                    OS = transactionRequest.OS,
                    SdkVersion = transactionRequest.SdkVersion,
                    SystemLanguage = transactionRequest.SystemLanguage,
                    Timezone = transactionRequest.Timezone
                }
            };

            var queueAddress = subscriberService.IsExtraSubscriber(transactionRequest.Subscriber) ? appSettings.ExtraQueueName : appSettings.QueueName;

            await massTransitProducer.SendAsync(message, queueAddress);
        }
    }
}
