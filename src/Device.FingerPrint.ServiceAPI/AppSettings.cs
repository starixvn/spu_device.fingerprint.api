using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.ServiceAPI
{
    public interface IAppSettings
    {
        string ApiUrl { get; }

        string EncryptPassword { get; }

        string SPName { get; }

        string SentryDsn { get; }

        string Environment { get; }

        string TelegramProxy { get; }

        string RatioUrl { get; }

        T GetValue<T>(string key);
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string apiUrl;

        private readonly string spName;

        private readonly string encryptPassword;

        private readonly string sentryDsn;

        private readonly string environment;

        private readonly string telegramProxy;

        private readonly string ratioUrl;

        private readonly string rabitMQHost;

        private readonly ushort rabitMQPort;

        private readonly string rabitMQVirtualHost;

        private readonly string rabitMQUserName;

        private readonly string rabitMQPassword;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            apiUrl = GetValue<string>("AppConfig:ApiUrl");

            encryptPassword = GetValue<string>("AppConfig:EncryptPassword");

            spName = GetValue<string>("AppConfig:SPName");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            environment = GetValue<string>("AppConfig:Environment");

            telegramProxy = GetValue<string>("TelegramProxy");

            ratioUrl = GetValue<string>("RatioUrl");

            rabitMQHost = GetValue<string>("RabbitMQ:Host");

            rabitMQPort = GetValue<ushort>("RabbitMQ:Port");

            rabitMQVirtualHost = GetValue<string>("RabbitMQ:VirtualHost");

            rabitMQUserName = GetValue<string>("RabbitMQ:UserName");

            rabitMQPassword = GetValue<string>("RabbitMQ:Password");
        }

        public string ApiUrl => apiUrl;

        public string EncryptPassword => encryptPassword;

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public string TelegramProxy => telegramProxy;

        public string RatioUrl => ratioUrl;

        public string RabitMQHost => rabitMQHost;

        public ushort RabitMQPort => rabitMQPort;

        public string RabitMQVirtualHost => rabitMQVirtualHost;

        public string RabitMQUserName => rabitMQUserName;

        public string RabitMQPassword => rabitMQPassword;

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

        public T GetSection<T>(string key)
        {
            var schedulerSection = configuration.GetSection(key);

            return schedulerSection.Get<T>();
        }
    }
}
