using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;

namespace Device.FingerPrint.Core
{
    public interface IAppSettings
    {
        string ApiUrl { get; }

        string PortalApiUrl { get; }

        string CryptoPassword { get; }

        string CryptoIV { get; }

        string CryptoSalt { get; }

        string SPName { get; }

        string SentryDsn { get; }

        string Environment { get; }

        bool IgnoreClientTransLogs { get; }

        long RedisExpired { get; }

        string JWTPassword { get; }

        string LogTrailBaseUrl { get; }
        
        string SystemName {get; }

        IConfiguration Configuration { get; }

        string RabitMQHost { get; }

        ushort RabitMQPort { get; }

        string RabitMQUserName { get; }

        string RabitMQPassword { get; }

        bool PushTransToMonitor { get; }

        bool BotDBotComponentForHuman { get; }

        T GetValue<T>(string key);
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string cdnApiUrl;
        private readonly string portalApiUrl;
        private readonly string apiUrl;
        private readonly string spName;
        private readonly string cryptoPassword;
        private readonly string sentryDsn;
        private readonly string environment;

        private readonly string cryptoIV;
        private readonly string cryptoSalt;

        private readonly bool ignoreClientTransLogs;
        private readonly long redisExpired;

        private readonly string jWTPassword;

        private readonly string logTrailBaseUrl;
        private readonly string systemName;

        private readonly string rabitMQHost;
        private readonly ushort rabitMQPort;
        private readonly string rabitMQVirtualHost;
        private readonly string rabitMQUserName;
        private readonly string rabitMQPassword;

        private readonly bool pushTransToMonitor;
        private readonly bool botDBotComponentForHuman;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            apiUrl = GetValue<string>("AppConfig:ApiUrl") ?? string.Empty;

            cdnApiUrl = GetValue<string>("AppConfig:CDNApiUrl") ?? string.Empty;

            portalApiUrl = GetValue<string>("AppConfig:PortalApiUrl") ?? string.Empty;

            cryptoPassword = GetValue<string>("AppConfig:Crypto:Password") ?? string.Empty;

            cryptoIV = GetValue<string>("AppConfig:Crypto:IV") ?? string.Empty;

            cryptoSalt = GetValue<string>("AppConfig:Crypto:Salt") ?? string.Empty;

            spName = GetValue<string>("AppConfig:SPName") ?? string.Empty;

            sentryDsn = GetValue<string>("Sentry:Dsn") ?? string.Empty;

            environment = GetValue<string>("AppConfig:Environment") ?? string.Empty;

            ignoreClientTransLogs = !string.IsNullOrEmpty(GetValue<string>("IgnoreClientTransLog")) && Convert.ToBoolean(GetValue<string>("IgnoreClientTransLog"));

            redisExpired = Convert.ToInt32(string.IsNullOrEmpty(GetValue<string>("Redis:Expired")) ? "604800" : GetValue<string>("Redis:Expired"));

            jWTPassword = GetValue<string>("AppConfig:JWT:Password") ?? "!QAZxdr567890";

            logTrailBaseUrl = GetValue<string>("LogTrail:BaseUrl") ?? string.Empty;

            systemName = GetValue<string>("LogTrail:SystemName") ?? string.Empty;

            rabitMQHost = GetValue<string>("RabbitMQ:Host");

            rabitMQPort = GetValue<ushort>("RabbitMQ:Port");

            rabitMQVirtualHost = GetValue<string>("RabbitMQ:VirtualHost");

            rabitMQUserName = GetValue<string>("RabbitMQ:UserName");

            rabitMQPassword = GetValue<string>("RabbitMQ:Password");

            botDBotComponentForHuman = GetValue<bool>("Consumer:TurnOnBotComponentForHuman");

            pushTransToMonitor = GetValue<bool>("RabbitMQ:Monitor:PushTrans-To-Monitor");
        }

        public string ApiUrl => apiUrl;

        public string CryptoPassword => cryptoPassword;

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string Environment => environment;

        public string CryptoIV => cryptoIV;

        public string CryptoSalt => cryptoSalt;

        public string CDNApiUrl => cdnApiUrl;

        public string PortalApiUrl => portalApiUrl;

        public bool IgnoreClientTransLogs => ignoreClientTransLogs;

        public long RedisExpired => redisExpired;

        public string JWTPassword => jWTPassword;

        public string LogTrailBaseUrl => logTrailBaseUrl;

        public string SystemName => systemName;


        public string RabitMQHost => rabitMQHost;

        public ushort RabitMQPort => rabitMQPort;

        public string RabitMQVirtualHost => rabitMQVirtualHost;

        public string RabitMQUserName => rabitMQUserName;

        public string RabitMQPassword => rabitMQPassword;

        public bool BotDBotComponentForHuman => botDBotComponentForHuman;

        public bool PushTransToMonitor => pushTransToMonitor;

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
