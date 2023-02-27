using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.Fingerprint.WebAPI
{
    public interface IAppSettings
    {
        string ApiUrl { get; }

        string CDNApiUrl { get; }

        string CryptoPassword { get; }

        string CryptoIV { get; }

        string CryptoSalt { get; }

        string SPName { get; }

        string SentryDsn { get; }

        string Environment { get; }       
        
        bool IgnoreClientTransLogs { get; }

    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string cdnApiUrl;
        private readonly string apiUrl;
        private readonly string spName;
        private readonly string cryptoPassword;
        private readonly string sentryDsn;
        private readonly string environment;

        private readonly string cryptoIV;
        private readonly string cryptoSalt;

        private readonly bool ignoreClientTransLogs;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            apiUrl = GetValue<string>("AppConfig:ApiUrl");

            cdnApiUrl = GetValue<string>("AppConfig:CDNApiUrl");

            cryptoPassword = GetValue<string>("AppConfig:Crypto:Password");

            cryptoIV = GetValue<string>("AppConfig:Crypto:IV");

            cryptoSalt = GetValue<string>("AppConfig:Crypto:Salt");

            spName = GetValue<string>("AppConfig:SPName");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            environment = GetValue<string>("AppConfig:Environment");

            ignoreClientTransLogs = GetValue<bool>("IgnoreClientTransLog");

        }

        public string ApiUrl => apiUrl;        

        public string CryptoPassword => cryptoPassword;

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public string CryptoIV => cryptoIV;

        public string CryptoSalt => cryptoSalt;

        public string CDNApiUrl => cdnApiUrl;

        public bool IgnoreClientTransLogs => ignoreClientTransLogs;

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
