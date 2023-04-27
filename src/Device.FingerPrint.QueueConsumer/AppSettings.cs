using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.QueueConsumer
{
    public interface IAppSettings
    {
        string ApiUrl { get; }

        string SPName { get; }

        string SentryDsn { get; }

        string Environment { get; }

        int TransBatchSize { get; }

        IConfiguration Configuration { get; }

    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly string apiUrl;
        private readonly string sentryDsn;
        private readonly string environment;
        private readonly int transBatchSize;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            apiUrl = GetValue<string>("AppConfig:ApiUrl");

            spName = GetValue<string>("AppConfig:SPName");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            environment = GetValue<string>("AppConfig:Environment");

            transBatchSize = GetValue<int>("Consumer:TransBatchSize");
        }

        public string ApiUrl => apiUrl;

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public int TransBatchSize => transBatchSize;

        public IConfiguration Configuration => configuration;

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
