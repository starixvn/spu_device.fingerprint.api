using System.ComponentModel;

namespace Device.FingerPrint.LogTrail.Consumer
{
    public interface IAppSettings
    {
        IConfiguration Configuration { get; }

        string SPName { get; }

        int TransBatch { get; }

        int QueueConsume { get; }

        string RabitMQHost { get; }

        string RabitMQPort { get; }

        string RabitMQVirtualHost { get; }

        string RabitMQPrefetchCount { get; }

        string RabitMQUserName { get; }

        string RabitMQPassword { get; }

        string RabitMQExchange { get; }

        string RabitMQName { get; }

        string RabitMQRoutingKey { get; }
    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly int transBatch;
        private readonly int queueConsume;
        private readonly string sentryDsn;
        private readonly string rabitMQHost;
        private readonly string rabitMQPort;
        private readonly string rabitMQVirtualHost;
        private readonly string rabitMQPrefetchCount;
        private readonly string rabitMQUserName;
        private readonly string rabitMQPassword;
        private readonly string rabitMQExchange;
        private readonly string rabitMQName;
        private readonly string rabitMQRoutingKey;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            spName = GetValue<string>("AppConfig:SPName");

            transBatch = GetValue<int>("ConsumerConfig:TransBatch");

            queueConsume = GetValue<int>("ConsumerConfig:Queue-Consume");

            rabitMQHost = GetValue<string>("RabbitMQ:Host");

            rabitMQPort = GetValue<string>("RabbitMQ:Port");

            rabitMQVirtualHost = GetValue<string>("RabbitMQ:VirtualHost");

            rabitMQPrefetchCount = GetValue<string>("RabbitMQ:PrefetchCount");

            rabitMQUserName = GetValue<string>("RabbitMQ:UserName");

            rabitMQPassword = GetValue<string>("RabbitMQ:Password");

            rabitMQExchange = GetValue<string>("RabbitMQ:Exchange");

            rabitMQName = GetValue<string>("RabbitMQ:Name");

            rabitMQRoutingKey = GetValue<string>("RabbitMQ:RoutingKey");

            sentryDsn = GetValue<string>("Sentry:Dsn");
        }

        public IConfiguration Configuration => configuration;

        public string SPName => spName;

        public int TransBatch => transBatch;

        public int QueueConsume => queueConsume;

        public string RabitMQHost => rabitMQHost;

        public string RabitMQPort => rabitMQPort;

        public string RabitMQVirtualHost => rabitMQVirtualHost;

        public string RabitMQPrefetchCount => rabitMQPrefetchCount;

        public string RabitMQUserName => rabitMQUserName;

        public string RabitMQPassword => rabitMQPassword;

        public string RabitMQExchange => rabitMQExchange;

        public string RabitMQName => rabitMQName;

        public string RabitMQRoutingKey => rabitMQRoutingKey;

        public string SentryDsn => sentryDsn;

        public T GetValue<T>(string key)
        {
            try
            {
                var value = configuration[key];

                var typeConverter = TypeDescriptor.GetConverter(typeof(T));

                return (T)typeConverter.ConvertFromString(value);
            }
            catch (Exception ex)
            {
                throw new InvalidCastException($"Key: {key}", ex);
            }
        }
    }
}
