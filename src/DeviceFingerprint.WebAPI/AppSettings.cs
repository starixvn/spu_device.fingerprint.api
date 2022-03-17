using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.Fingerprint.WebAPI
{
    public interface IAppSettings
    {
        string ApiUrl { get; }

        string EncryptPassword { get; }

        string SPName { get; }

        string SentryDsn { get; }

        string Environment { get; }

        string BootstrapServers { get; }

        string ServerTransTopicName { get; }

        string ClientTransTopicName { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string apiUrl;
        private readonly string spName;
        private readonly string encryptPassword;
        private readonly string sentryDsn;
        private readonly string environment;

        private readonly string bootstrapServers;
        private readonly string serverTransTopicName;
        private readonly string clientTransTopicName;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            apiUrl = GetValue<string>("AppConfig:ApiUrl");

            encryptPassword = GetValue<string>("AppConfig:EncryptPassword");

            spName = GetValue<string>("AppConfig:SPName");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            environment = GetValue<string>("AppConfig:Environment");

            bootstrapServers = GetValue<string>("Kafka:ClientConfigs:BootstrapServers");

            serverTransTopicName = GetValue<string>("Kafka:ClientConfigs:ServerTransTopicName");

            clientTransTopicName = GetValue<string>("Kafka:ClientConfigs:ClientTransTopicName");
        }

        public string ApiUrl => apiUrl;

        public string EncryptPassword => encryptPassword;

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public string BootstrapServers => bootstrapServers;

        public string ServerTransTopicName => serverTransTopicName;

        public string ClientTransTopicName => clientTransTopicName;

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
