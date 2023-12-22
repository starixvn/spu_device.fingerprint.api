namespace Device.FingerPrint.LogTrail.Consumer.Consumer
{
    using Device.FingerPrint.LogTrail.Consumer.Services;
    using Fanex.Logging;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Extensions;
    using MassTransit;
    using Device.FingerPrint.Core.Model.LogEvent;

    public class LogTrailFaultConsumer : IConsumer<Batch<Fault<LogEventInfo>>>
    {
        private readonly ICapturingService capturingService;
        private readonly ILogger logger;

        public LogTrailFaultConsumer(
            ICapturingService capturingService,
            ILogger logger)
        {
            this.capturingService = capturingService;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<Batch<Fault<LogEventInfo>>> context)
        {
            try
            {
                logger.Info("Start Consuming log trail fault !");

                var logEventInfos = context.Message.Select(x =>
                {
                    var message = x.Message.Message;
                    
                    return new LogEventInfo
                    {
                        EventDate = message.EventDate,
                        SubscriberID = message.SubscriberID,
                        LoginName = message.LoginName,
                        TransTime = message.TransTime,
                        EventCode = message.EventCode,
                        EventCodeDetail = message.EventCodeDetail,
                        SystemName = message.SystemName,
                        Source = message.Source,
                        Destination = message.Destination,
                        Method = message.Method,
                    };
                });

                await capturingService.InsertLogTrailEventAsync(logEventInfos);

                await capturingService.InsertLogTrailEventDetailAsync(logEventInfos);
            }
            catch (Exception ex)
            {
                HandleErrorException(ex);

                throw;
            }
        }

        private void HandleErrorException(Exception ex)
        {
            var logMessage = new LogMessage(ex.Message)
                       .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                       .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                       .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                       .ToString();

            logger.Error(logMessage);
        }
    }
}
