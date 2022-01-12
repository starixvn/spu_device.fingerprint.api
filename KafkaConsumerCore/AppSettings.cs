using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace KafkaConsumerCore
{
    public interface IAppSettings
    {
        string ApiUrl { get; }
        string SPName { get; }
        string SentryDsn { get; }
        string Environment { get; }
        string TopicName { get; }
        string ConsumerGroupName { get; }
        string BootstrapServers { get; }

    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly string apiUrl;
        private readonly string sentryDsn;
        private readonly string environment;
        private readonly string topicName;
        private readonly string consumerGroupName;
        private readonly string bootstrapServers;
        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;
            apiUrl = GetValue<string>("AppConfig:ApiUrl");
            spName = GetValue<string>("AppConfig:SPName");
            sentryDsn = GetValue<string>("Sentry:Dsn");
            environment = GetValue<string>("AppConfig:Environment");
            topicName = GetValue<string>("KafkaConfig:TopicName");
            consumerGroupName = GetValue<string>("KafkaConfig:ConsumerGroupName");
            bootstrapServers = GetValue<string>("KafkaConfig:BootstrapServers");
        }
        public string ApiUrl => apiUrl;
        public string SPName => spName;
        public string SentryDsn => sentryDsn;
        public string Environment => environment;
        public string TopicName => topicName;
        public string ConsumerGroupName => consumerGroupName;
        public string BootstrapServers => bootstrapServers;
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
