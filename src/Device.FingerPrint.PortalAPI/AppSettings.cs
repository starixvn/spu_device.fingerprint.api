using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.PortalAPI
{
    public interface IAppSettings
    {
        string CryptoPassword { get; }

        string CryptoIV { get; }

        string CryptoSalt { get; }

        string SPName { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string cryptoPassword;
        private readonly string cryptoIV;
        private readonly string cryptoSalt;
        private readonly string spName;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            cryptoPassword = GetValue<string>("AppConfig:Crypto:Password");

            cryptoIV = GetValue<string>("AppConfig:Crypto:IV");

            cryptoSalt = GetValue<string>("AppConfig:Crypto:Salt");

            spName = GetValue<string>("AppConfig:SPName");
        }

        public string CryptoPassword => cryptoPassword;

        public string CryptoIV => cryptoIV;

        public string CryptoSalt => cryptoSalt;

        public string SPName => spName;

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
