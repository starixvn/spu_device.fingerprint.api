using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.Gateway
{
    public interface IAppSettings
    {

        bool IsUM { get; }

        string SentryDsn { get; }

        string GetSubscriberInfo { get; }

        string LogstashUrl { get; }

        string Environment { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly bool isUM;

        private readonly string sentryDsn;

        private readonly string getSubscriberInfo;

        private readonly string logstashUrl;

        private readonly string environment;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            isUM = GetValue<bool>("IsUM");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            getSubscriberInfo = GetValue<string>("GetSubscriberUrl");

            logstashUrl = GetValue<string>("Logstash:Url");

            environment = GetValue<string>("Environment");
        }

        public bool IsUM => isUM;

        public string SentryDsn => sentryDsn;

        public string GetSubscriberInfo => getSubscriberInfo;

        public string LogstashUrl => logstashUrl;

        public string Environment => environment;

        public T GetValue<T>(string key)
        {
            try
            {
                var value = configuration[key];

                if (value == null)
                {
                    throw new InvalidCastException($"Key: {key}");
                }

                // For string types, allow empty strings
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)value;
                }

                var typeConverter = TypeDescriptor.GetConverter(typeof(T));

                if (typeof(T) == typeof(bool))
                {
                    // Handle various boolean representations
                    if (bool.TryParse(value, out bool boolResult))
                    {
                        return (T)(object)boolResult;
                    }
                    
                    switch (value.ToLowerInvariant())
                    {
                        case "1":
                        case "yes":
                            return (T)(object)true;
                        case "0":
                        case "no":
                            return (T)(object)false;
                        default:
                            throw new InvalidCastException($"Key: {key}");
                    }
                }

                return (T)typeConverter.ConvertFromString(value);
            }
            catch (Exception ex) when (!(ex is InvalidCastException))
            {
                throw new InvalidCastException($"Key: {key}", ex);
            }
        }
    }
}
