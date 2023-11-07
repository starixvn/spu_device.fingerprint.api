namespace Device.FingerPrint.LogTrail.Consumer.Consumer
{
    using Device.FingerPrint.LogTrail.Consumer.Services;
    using Fanex.Logging;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Extensions;
    using MassTransit;
    using Device.FingerPrint.Core.Model;

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

                var rawLogEvent = context.Message.Select(x =>
                {
                    var message = x.Message.Message;
                    return new LogEventInfo
                    {
                        EventDate = message.EventDate,
                        SubscriberID = message.SubscriberID,
                        SystemName = message.SystemName,
                        Source = message.Source,
                        Destination = message.Destination,
                        Method = message.Method,
                    };
                });

                await capturingService.InserLogTrailEventAsync(rawLogEvent);
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
