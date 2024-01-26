using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace FPS.RawTransaction.Insert
{
    public interface IAppSettings
    {
        string SPName { get; }

        IConfiguration Configuration { get; }

        T GetValue<T>(string key);
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string spName;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            spName = GetValue<string>("AppConfig:SPName") ?? string.Empty;
        }

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

        public IConfiguration Configuration => configuration;

    }
}