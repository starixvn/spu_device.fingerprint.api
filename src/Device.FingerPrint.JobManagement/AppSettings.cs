using Device.FingerPrint.JobManagement.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.JobManagement
{
    public interface IAppSettings
    {
        string SentryDsn { get; }

        string Environment { get; }

        string TelegramProxy { get; }

        string TelegramGroupID { get; }

        Scheduler Schedulers { get; }
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string sentryDsn;
        private readonly string environment;
        private readonly string telegramProxy;
        private readonly string telegramGroupID;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            sentryDsn = GetValue<string>("Sentry:Dsn");

            environment = GetValue<string>("Environment");

            telegramProxy = GetValue<string>("Telegram:Proxy");

            telegramGroupID = GetValue<string>("Telegram:GroupID");

            Schedulers = GetSection<Scheduler>(nameof(Schedulers));
        }

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public string TelegramProxy => telegramProxy;

        public string TelegramGroupID => telegramGroupID;

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
