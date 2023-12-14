namespace Device.FingerPrint.LogTrail.Consumer.Consumer
{
    using System;
    using System.Threading.Tasks;
    using Fanex.Logging;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Extensions;
    using MassTransit;
    using Device.FingerPrint.LogTrail.Consumer.Services;
    using System.Linq;
    using Device.FingerPrint.Core.Model;

    public class LogTrailConsumer : IConsumer<Batch<LogEventInfo>>
    {
        private readonly ICapturingService capturingService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        public LogTrailConsumer(
            ICapturingService capturingService,
            IAppSettings appSettings,
            ILogger logger)
        {
            this.capturingService = capturingService;
            this.appSettings = appSettings;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<Batch<LogEventInfo>> context)
        {
            try
            {
                if (appSettings.GetValue<bool>("RabbitMQ:LogTrail:Queue-Consume"))
                {
                    var rawLogEvent = context.Message.Select(x =>
                    {
                        return new LogEventInfo
                        {
                            EventDate = x.Message.EventDate,
                            SubscriberID = x.Message.SubscriberID,
                            SystemName = x.Message.SystemName,
                            Source = x.Message.Source,
                            Destination = x.Message.Destination,
                            Method = x.Message.Method,
                        };
                    });

                    await capturingService.InserLogTrailEventAsync(rawLogEvent);
                }
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
