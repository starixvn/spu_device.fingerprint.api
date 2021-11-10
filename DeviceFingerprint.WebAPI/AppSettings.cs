using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI
{
    public interface IAppSettings
    {
        string ApiUrl { get; }

        string EncryptPassword { get; }

        string SPName { get; }
    }

    public class AppSettings: IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string apiUrl;
        private readonly string spName;
        private readonly string encryptPassword;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;
                        
            apiUrl = GetValue<string>("AppConfig:ApiUrl");

            encryptPassword = GetValue<string>("AppConfig:EncryptPassword");

            spName = GetValue<string>("AppConfig:SPName");
        }

        public string ApiUrl => apiUrl;

        public string EncryptPassword => encryptPassword;

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
