using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.Kafka
{
    public interface IAppSettings
    {
        string ApiUrl { get; }
        string SPName { get; }
        string SentryDsn { get; }
        string Environment { get; }

        string BootstrapServers { get; }
        string NumberOfRecordsProcessIn1Time { get; }
        string TopicName { get; }
        string ConsumerGroupName { get; }

    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly string apiUrl;
        private readonly string sentryDsn;
        private readonly string environment;

        private readonly string bootstrapServers;
        private readonly string numberOfRecordsProcessIn1Time;
        private readonly string topicName;
        private readonly string consumerGroupName;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;
            apiUrl = GetValue<string>("AppConfig:ApiUrl");
            spName = GetValue<string>("AppConfig:SPName");
            sentryDsn = GetValue<string>("Sentry:Dsn");
            environment = GetValue<string>("AppConfig:Environment");

            bootstrapServers = GetValue<string>("KafkaConfig:BootstrapServers");
            numberOfRecordsProcessIn1Time = GetValue<string>("KafkaConfig:NumberOfRecordsProcessIn1Time");
            topicName = GetValue<string>("KafkaConfig:TopicName");
            consumerGroupName = GetValue<string>("KafkaConfig:ConsumerGroupName");
        }
        public string ApiUrl => apiUrl;
        public string SPName => spName;
        public string SentryDsn => sentryDsn;
        public string Environment => environment;

        public string BootstrapServers => bootstrapServers;
        public string NumberOfRecordsProcessIn1Time => numberOfRecordsProcessIn1Time;
        public string TopicName => topicName;
        public string ConsumerGroupName => consumerGroupName;

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
