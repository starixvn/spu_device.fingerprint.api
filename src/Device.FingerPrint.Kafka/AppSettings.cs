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

        int ConsumerBatchSize { get; }

        string KafkaTopic { get; }
    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly string apiUrl;
        private readonly string sentryDsn;
        private readonly string environment;
        private readonly int consumerBatchSize;
        private readonly string kafkaTopic;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            apiUrl = GetValue<string>("AppConfig:ApiUrl");

            spName = GetValue<string>("AppConfig:SPName");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            environment = GetValue<string>("AppConfig:Environment");

            consumerBatchSize = GetValue<int>("Kafka:Consumer:BatchSize");

            kafkaTopic = GetValue<string>("Kafka:ClientConfigs:Topic");
        }

        public string ApiUrl => apiUrl;

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public int ConsumerBatchSize => consumerBatchSize;

        public string KafkaTopic => kafkaTopic;

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
