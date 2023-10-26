namespace Device.FingerPrint.LogTrail.Consumer.Handler
{
    using Device.FingerPrint.Core.Model;
    using Device.FingerPrint.LogTrail.Consumer.Services;
    using Device.FingerPrint.LogTrail.Model;
    using Device.FingerPrint.Queue;
    using EasyNetQ;
    using Fanex.Logging;

    public class Consumer : BackgroundService
    {
        private readonly ICapturingService capturingService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;


        public Consumer(ICapturingService capturingService, IAppSettings appSettings, ILogger logger)
        {
            this.capturingService = capturingService;
            this.appSettings = appSettings;
            this.logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming();

            return Task.CompletedTask;
        }

        private void StartConsuming()
        {
            try
            {
                if (appSettings.QueueConsume == 1)
                {
                    logger.Info("Start Consuming!");

                    var easyNetQConnector = RabbitMQBus();
                    var rawLogEvents = new List<RawLogEventModel>();

                    easyNetQConnector.AdvanceBus.Consume(easyNetQConnector.Queue, (Action<IMessage<LogEventInfo>, MessageReceivedInfo>)((message, info) =>
                    {
                        try
                        {
                            if (message.Body != null)
                            {
                                var rawLogEvent = TransformRawLogEvent(message.Body);
                                rawLogEvents.Add(rawLogEvent);

                                if (rawLogEvents.Count >= appSettings.TransBatch)
                                {
                                    var groupRawLogEvents = rawLogEvents
                                    .GroupBy(x => new
                                    {
                                        x.Date.Date,
                                        x.SubscriberID,
                                        x.Source,
                                        x.Method,
                                        x.SystemName
                                    })
                                    .Select(group => new RawLogEventModel
                                    {
                                        Date = group.Key.Date.Date,
                                        SubscriberID = group.Key.SubscriberID,
                                        Source = group.Key.Source,
                                        Method = group.Key.Method,
                                        SystemName = group.Key.SystemName,
                                        Total = group.Count()
                                    });

                                    capturingService.InserLogTrailEvent(groupRawLogEvents);

                                    rawLogEvents.Clear();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private RawLogEventModel TransformRawLogEvent(LogEventInfo logEventInfo)
        {
            var rawLogEvent = new RawLogEventModel
            {
                Date = logEventInfo.Date,
                SubscriberID = logEventInfo.SubscriberID,
                Source = logEventInfo.Destination,
                Method = logEventInfo.Method,
                SystemName = logEventInfo.SystemName,
            };

            return rawLogEvent;
        }
        private EasyNetQConnector RabbitMQBus()
        {
            var easyNetQConnector = new EasyNetQConnector(appSettings.RabitMQHost + ":" + appSettings.RabitMQPort,
                                      appSettings.RabitMQVirtualHost,
                                      int.Parse(appSettings.RabitMQPrefetchCount),
                                      appSettings.RabitMQUserName,
                                      appSettings.RabitMQPassword);

            easyNetQConnector
                    .CreateBus()
                    .DeclareExchange(appSettings.RabitMQExchange)
                    .DeclareQueue(appSettings.RabitMQName)
                    .DeclareRoutingKey(appSettings.RabitMQRoutingKey)
                    .Bind();

            return easyNetQConnector;
        }
    }
}
