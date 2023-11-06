using System.ComponentModel;

namespace Device.FingerPrint.LogTrail.Consumer
{
    public interface IAppSettings
    {
        IConfiguration Configuration { get; }

        string SPName { get; }

        string RabitMQHost { get; }

        ushort RabitMQPort { get; }

        string RabitMQUserName { get; }

        string RabitMQPassword { get; }

        T GetValue<T>(string key);
    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly string sentryDsn;
        private readonly string rabitMQHost;
        private readonly ushort rabitMQPort;
        private readonly string rabitMQVirtualHost;
        private readonly string rabitMQUserName;
        private readonly string rabitMQPassword;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            spName = GetValue<string>("AppConfig:SPName");

            rabitMQHost = GetValue<string>("RabbitMQ:Host");

            rabitMQPort = GetValue<ushort>("RabbitMQ:Port");

            rabitMQVirtualHost = GetValue<string>("RabbitMQ:VirtualHost");

            rabitMQUserName = GetValue<string>("RabbitMQ:UserName");

            rabitMQPassword = GetValue<string>("RabbitMQ:Password");


            sentryDsn = GetValue<string>("Sentry:Dsn");
        }

        public IConfiguration Configuration => configuration;

        public string SPName => spName;

        public string RabitMQHost => rabitMQHost;

        public ushort RabitMQPort => rabitMQPort;

        public string RabitMQVirtualHost => rabitMQVirtualHost;

        public string RabitMQUserName => rabitMQUserName;

        public string RabitMQPassword => rabitMQPassword;

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
