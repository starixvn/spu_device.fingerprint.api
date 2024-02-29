namespace Device.FingerPrint.TransactionAPI.Controllers
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.Redis;
    using Device.FingerPrint.TransactionAPI.Model;
    using Device.FingerPrint.TransactionAPI.Services;
    using Fanex.Logging;
    using Fanex.LogTrail.Service;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using ONEbook.UM.Services;

    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IMaintenanceService maintenanceService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;
        private readonly ILogEvent logEvent;
        private readonly IRedisService redisService;
        private readonly ISubscriberService subscriberService;
        private readonly IMassTransitProducer massTransitProducer;

        private const string defaultDestination = "RabbitMQ";
        private const string defaultMethod = "PushQueue";
        private const string defaultSource = "API";

        public TransactionController(
            IMaintenanceService maintenanceService,
            IAppSettings appSettings,
            ILogger logger,
            ILogEvent logEvent,
            IRedisService redisService,
            ISubscriberService subscriberService,
            IMassTransitProducer massTransitProducer)
        {
            this.maintenanceService = maintenanceService;
            this.appSettings = appSettings;
            this.logger = logger;
            this.logEvent = logEvent;
            this.redisService = redisService;
            this.subscriberService = subscriberService;
            this.massTransitProducer = massTransitProducer;
        }

        [HttpPost("FPS/AddTransaction")]
        [HttpPost("FPS/AddTransactionVer2")]
        [HttpPost("FPS/CheckTransaction")]
        public async Task<IActionResult> CheckTransactionAsync(TransactionRequest transRequest)
        {
            var responseCode = TransactionResult.UnderMaintenance;

            if (!IsMaintenance && !IsIgnoredSite(transRequest.Subscriber))
            {
                try
                {
                    if(transRequest.Subscriber.ToLower() == "12bet")
                    {
                        logger.Error("this is 12bet transaction: " + transRequest.Account);
                    }

                    responseCode = await subscriberService.ValidateAsync(transRequest, IsExtraSubscriber(transRequest.Subscriber));

                    if (responseCode == TransactionResult.Success)
                    {
                        var di = transRequest.Di ?? transRequest.__di;
                        var transAttributesJoined = JoinStringAttributes(new List<string> {
                        transRequest.AcceptsTypes,
                        transRequest.Platform,
                        (DateTime.UtcNow - DateTime.Now).TotalMinutes.ToString(),
                        transRequest.UserAgent,
                        transRequest.Screen,
                        transRequest.Ip,
                        di,
                        transRequest.Url,
                        transRequest.DetecasAnalysis
                    }, "###");

                        await PushToQueueAsync(transRequest, transAttributesJoined);

                        if (appSettings.LogTrailEnable)
                        {
                            _ = InsertLogEventAsync(transRequest.SubscriberId);
                        }
                    }
                    else
                    {
                        var errorMessage = GetTransactionResponse(responseCode);
                        await logger.ErrorAsync($"CheckTransactionAsync responseCode = {JsonSerializer.Serialize(errorMessage)} \nTransRequest: {JsonSerializer.Serialize(transRequest)}");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message, ex);

                    responseCode = TransactionResult.Others;
                }
            }

            var response = GetTransactionResponse(responseCode);
            return Ok(response.ResultMessage);
        }

        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            var httpConnectionFeature = HttpContext.Features.Get<IHttpConnectionFeature>();
            var localIpAddress = httpConnectionFeature?.LocalIpAddress;
            return Ok(new WhoAmIModel
            {
                LocalIP = localIpAddress.ToString(),
                ServerName = Environment.MachineName,
                ServerTime = DateTime.Now
            });
        }

        private bool IsIgnoredSite(string site)
        {
            var ignoredSites = appSettings.IgnoredSites.Split(',');

            if (ignoredSites[0].Equals("*", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return ignoredSites.Any(s => s.Equals(site, StringComparison.OrdinalIgnoreCase));
        }

        private string JoinStringAttributes(List<string> attributes, string separator)
        {
            return string.Join(separator, attributes.ToArray());
        }

        private async Task PushToQueueAsync(TransactionRequest transactionRequest, string transAttributesJoined)
        {
            var dataEvent = new MessageQueue
            {
                Account = transactionRequest.Account,
                Device = transAttributesJoined,
                Action = transactionRequest.Action,
                ActionResult = transactionRequest.ActionResult,
                Subscriber = transactionRequest.Subscriber,
                SubscriberId = transactionRequest.SubscriberId,
                IP = transactionRequest.Ip,
                FakeIP = transactionRequest.FakeIp,
                TransDate = DateTime.Now,
                SubscriberDate = transactionRequest.SubscriberDate,
                Token = transactionRequest.Token,
                Recaptcha = "",
                SessionKey = transactionRequest.SessionKey,
                BotD = transactionRequest.Bi
            };

            var queueAddress = IsExtraSubscriber(transactionRequest.Subscriber) ? appSettings.ExtraQueueName : appSettings.QueueName;

            await massTransitProducer.SendAsync(dataEvent, queueAddress);
        }

        private async Task InsertLogEventAsync(int subscriberId)
        {
            var logEventInfo = new LogEventInfo
            {
                EventDate = DateTime.Now,
                SubscriberID = subscriberId,
                SystemName = appSettings.LogTrailSystemName,
                Source = defaultSource,
                Destination = defaultDestination,
                Method = defaultMethod,
            };

            await logEvent.WriteEventInfoAsync(logEventInfo);
        }

        private bool IsExtraSubscriber(string subscriberName)
        {
            if (appSettings.ExtraSubscribers != null && appSettings.ExtraSubscribers.Any())
            {
                return appSettings.ExtraSubscribers.Exists(x => x.SubscriberName.ToLower() == subscriberName.ToLower());
            }

            return false;
        }

        private TransactionResponse GetTransactionResponse(string code)
        {
            return new TransactionResponse
            {
                ResultCode = code,
                ResultMessage = TransactionResult.GetMessage(code)
            };
        }

        private bool IsMaintenance
        {
            get
            {
                var downTime = maintenanceService.GetUnderMaintenanceTime();

                return downTime.IsUnderMaintenanceTime || appSettings.IsUM;
            }
        }

        [HttpGet]
        [Route("FPS/TestRedis")]
        public async Task<string> TestRedisAsync(string data)
        {
            await redisService.AddAsyncAndExpiryAsync("test:testing", data, DateTimeOffset.UtcNow.AddMinutes(1));

            return await redisService.GetAsync<string>("test:testing");
        }

        [HttpGet]
        [Route("FPS/TestAPI/{message}")]
        public string TestAPI(string message)
        {
            try
            {
                logger.Error(message);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return message;
        }

    }
}
