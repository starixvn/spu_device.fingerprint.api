namespace Device.FingerPrint.RawTransMonitoring.QueueConsumer
{
    using System.ComponentModel;
    using System;
    using Microsoft.Extensions.Configuration;

    public interface IAppSettings
    {
        string SPName { get; }

        string SentryDsn { get; }

        string Environment { get; }

        bool IsConsumer { get; }

        IConfiguration Configuration { get; }

        T GetValue<T>(string key);
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string spName;
        private readonly string sentryDsn;
        private readonly string environment;
        private readonly bool isConsumer;


        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            spName = GetValue<string>("AppConfig:SPName") ?? string.Empty;

            sentryDsn = GetValue<string>("Sentry:Dsn") ?? string.Empty;

            environment = GetValue<string>("AppConfig:Environment") ?? string.Empty;

            isConsumer = !string.IsNullOrEmpty(GetValue<string>("Consumer:IsConsumer")) && Convert.ToBoolean(GetValue<string>("Consumer:IsConsumer"));
        }

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public bool IsConsumer => isConsumer;

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
