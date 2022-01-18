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
    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly string apiUrl;
        private readonly string sentryDsn;
        private readonly string environment;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;
            apiUrl = GetValue<string>("AppConfig:ApiUrl");
            spName = GetValue<string>("AppConfig:SPName");
            sentryDsn = GetValue<string>("Sentry:Dsn");
            environment = GetValue<string>("AppConfig:Environment");
        }
        public string ApiUrl => apiUrl;
        public string SPName => spName;
        public string SentryDsn => sentryDsn;
        public string Environment => environment;

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
