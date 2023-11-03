namespace Device.FingerPrint.LogTrail.Consumer.Handler
{
    using Device.FingerPrint.LogTrail.Consumer.Services;
    using Device.FingerPrint.Queue;
    using EasyNetQ;
    using Fanex.Logging;
    using FPS.Device.FingerPrint.LogTrail.Net.Standard.Client.Models;

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

                    var logEventInfos = new List<LogEventInfo>();

                    easyNetQConnector.AdvanceBus.Consume(easyNetQConnector.Queue, (Action<IMessage<LogEventInfo>, MessageReceivedInfo>)((message, info) =>
                    {
                        try
                        {
                            if (message.Body != null)
                            {
                                var logEvent = message.Body;
                                logEventInfos.Add(logEvent);

                                if (logEventInfos.Count >= appSettings.TransBatch)
                                {
                                    var groupLogEvents = logEventInfos
                                    .GroupBy(x => new
                                    {
                                        x.EventDate.Date,
                                        x.SubscriberID,
                                        x.SystemName,
                                        x.Source,
                                        x.Destination,
                                        x.Method,
                                    })
                                    .Select(group => new LogEventInfo
                                    {
                                        EventDate = group.Key.Date.Date,
                                        SubscriberID = group.Key.SubscriberID,
                                        SystemName = group.Key.SystemName,
                                        Source = group.Key.Source,
                                        Destination = group.Key.Destination,
                                        Method = group.Key.Method,
                                        Total = group.Count()
                                    });

                                    capturingService.InserLogTrailEventAsync(groupLogEvents);

                                    logEventInfos.Clear();
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
