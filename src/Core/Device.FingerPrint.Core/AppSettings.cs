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

        int TransBatchSize { get; }

        string LogTrailEndPoint { get; }
        
        int TransOfExtraSubsBatch { get; }

        bool BotDBotComponentForHuman { get; }

        string SystemName {get; }

        string ExtraSubs { get; }

        bool IsReadyDbExtra { get; }

        int PushTransToMonitor { get; }

        int Interval { get; }

    IConfiguration Configuration { get; }
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

        private readonly int transBatchSize;
        private readonly int transOfExtraSubsBatch;
        private readonly bool botDBotComponentForHuman;
        private readonly string extraSubs;
        private readonly bool isReadyDbExtra;
        private readonly int pushTransToMonitor;

        private readonly int interval;

        private readonly string logTrailEndPoint;
        private readonly string systemName;

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

            transBatchSize = Convert.ToInt32(string.IsNullOrEmpty(GetValue<string>("Consumer:TransBatchSize")) ? "0" : GetValue<string>("Consumer:TransBatchSize"));

            transOfExtraSubsBatch = Convert.ToInt32(string.IsNullOrEmpty(GetValue<string>("Consumer:TransOfExtraSubs-Batch")) ? "0" : GetValue<string>("Consumer:TransOfExtraSubs-Batch"));

            logTrailEndPoint = GetValue<string>("LogTrail:EndPoint") ?? string.Empty;

            systemName = GetValue<string>("LogTrail:SystemName") ?? string.Empty;
            
            botDBotComponentForHuman = !string.IsNullOrEmpty(GetValue<string>("Consumer:TurnOnBotComponentForHuman")) && Convert.ToBoolean(GetValue<string>("Consumer:TurnOnBotComponentForHuman"));

            isReadyDbExtra = !string.IsNullOrEmpty(GetValue<string>("Consumer:IsReadyDbExtra")) && Convert.ToBoolean(GetValue<string>("Consumer:IsReadyDbExtra"));

            extraSubs = GetValue<string>("Consumer:Extra-Subs") ?? "CTMax;Velki;AgentCTMax;Sky;agentvelki;AgentSky";

            pushTransToMonitor = Convert.ToInt32(string.IsNullOrEmpty(GetValue<string>("Consumer:PushTrans-To-Monitor")) ? "0" : GetValue<string>("Consumer:PushTrans-To-Monitor"));

            interval = Convert.ToInt32(string.IsNullOrEmpty(GetValue<string>("Consumer:Interval")) ? "0" : GetValue<string>("Consumer:Interval"));

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

        public int TransBatchSize => transBatchSize;

        public string LogTrailEndPoint => logTrailEndPoint;
        
        public int TransOfExtraSubsBatch => transOfExtraSubsBatch;

        public bool BotDBotComponentForHuman => botDBotComponentForHuman;
        
        public string SystemName => systemName;

        public string ExtraSubs => extraSubs;

        public bool IsReadyDbExtra => isReadyDbExtra;

        public int PushTransToMonitor => pushTransToMonitor;

        public int Interval => interval;

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
