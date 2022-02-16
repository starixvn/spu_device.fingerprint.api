using Device.FingerPrint.JobManagement.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.JobManagement
{
    public interface IAppSettings
    {
        string SPName { get; }

        string SentryDsn { get; }

        int Environment { get; }

        string EnvironmentString { get; }

        Scheduler Schedulers { get; }
    }

    public class AppSettings: IAppSettings
    {
        private readonly IConfiguration configuration;        
        private readonly string spName;        
        private readonly string sentryDsn;
        private readonly string environment;        

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            spName = GetValue<string>("AppConfig:SPName");

            sentryDsn = GetValue<string>("Sentry:Dsn");

            environment = GetValue<string>("AppConfig:Environment");

            Schedulers = GetSection<Scheduler>(nameof(Schedulers));
        }                

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public int Environment
        {
            get
            {
                switch (environment)
                {
                    case "LOCAL":
                        return 1;
                    case "UAT":
                        return 2;
                    default:
                        return 3;
                }
            }
        }

        public string EnvironmentString => environment;

        public Scheduler Schedulers { get; }

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
