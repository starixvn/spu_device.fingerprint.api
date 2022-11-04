using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.DCS.QueueConsumer
{
    public interface IAppSettings
    {
        IConfiguration Configuration { get; }

        string SPName { get; }

        int TransBatch { get; }
    }
    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;
        private readonly string spName;
        private readonly int transBatch;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            spName = GetValue<string>("AppConfig:SPName");

            transBatch = GetValue<int>("ConsumerConfig:TransBatch");
        }

        public IConfiguration Configuration => configuration;

        public string SPName => spName;

        public int TransBatch => transBatch;

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