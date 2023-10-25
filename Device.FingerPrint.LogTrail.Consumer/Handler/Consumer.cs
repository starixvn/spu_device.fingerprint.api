namespace Device.FingerPrint.LogTrail.Consumer.Handler
{
    using Device.FingerPrint.Core.Model;
    using Device.FingerPrint.Queue;
    using EasyNetQ;
    using Fanex.Logging;

    public class Consumer : BackgroundService
    {
        private readonly IAppSettings appSettings;
        private readonly ILogger _logger;

        public Consumer(IAppSettings appSettings, ILogger logger)
        {
            this.appSettings = appSettings;
            _logger = logger;
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
                    _logger.Info("Start Consuming!");

                    var easyNetQConnector = RabbitMQBus();
                    var logEvents = new List<LogEventInfo>();

                   
                    easyNetQConnector.AdvanceBus.Consume(easyNetQConnector.Queue, (Action<IMessage<LogEventInfo>, MessageReceivedInfo>)((message, info) =>
                    {
                        try
                        {
                            if (message.Body != null)
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex.ToString());
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
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
