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

        int RedisDatabase { get; }

        long RedisExpired { get; }

        string JWTPassword { get; }

        string JWT { get; }

        string JWTIssue { get; }

        string JWTAudience { get; }

        string LogTrailBaseUrl { get; }
        
        string SystemName {get; }

        bool IsEnableLogTrail { get; }

        bool IsExtraSub { get; }

        int MaxMonthsGetAssociationByDevice { get; }
        
        long EverCookieExpireSecond { get; }

        IConfiguration Configuration { get; }

        T GetValue<T>(string key);

        T GetSection<T>(string key);
    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        public const string DeviceIdEncryptKey = "&％#@?,:*!~^.";

        private readonly string cdnApiUrl;
        private readonly string portalApiUrl;
        private readonly string apiUrl;
        private readonly string spName;
        private readonly string cryptoPassword;
        private readonly string sentryDsn;
        private readonly string environment;

        private readonly string cryptoIV;
        private readonly string cryptoSalt;

        private readonly long redisExpired;
        private readonly int redisDatabase;

        private readonly string jWTPassword;

        private readonly string jWTIssue;

        private readonly string jWTAudience;

        private readonly string jWT;

        private readonly string logTrailBaseUrl;
        private readonly string systemName;
        private readonly bool isEnableLogTrail;

        private readonly bool isExtraSub;
        
        private readonly int maxMonthsGetAssociationByDevice;
        
        private readonly long everCookieExpireSecond;

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

            redisDatabase = Convert.ToInt32(string.IsNullOrEmpty(GetValue<string>("Redis:Database")) ? "0" : GetValue<string>("Redis:Database"));

            redisExpired = Convert.ToInt32(string.IsNullOrEmpty(GetValue<string>("Redis:Expired")) ? "604800" : GetValue<string>("Redis:Expired"));

            jWTPassword = GetValue<string>("AppConfig:JWT:Password") ?? "!QAZxdr567890";

            jWTIssue = GetValue<string>("AppConfig:JWT:Issue") ?? string.Empty;

            jWTAudience = GetValue<string>("AppConfig:JWT:Audience") ?? string.Empty;

            jWT = GetValue<string>("AppConfig:JWT") ?? string.Empty;

            logTrailBaseUrl = GetValue<string>("LogTrail:BaseUrl") ?? string.Empty;

            systemName = GetValue<string>("LogTrail:SystemName") ?? string.Empty;

            isEnableLogTrail = !string.IsNullOrEmpty(GetValue<string>("LogTrail:IsEnable")) && Convert.ToBoolean(GetValue<string>("LogTrail:IsEnable"));

            isExtraSub = !string.IsNullOrEmpty(GetValue<string>("Consumer:IsExtraSub")) && Convert.ToBoolean(GetValue<string>("Consumer:IsExtraSub"));

            maxMonthsGetAssociationByDevice = Convert.ToInt32(string.IsNullOrEmpty(GetValue<string>("API:MaxMonthsGetAssociationByDevice")) ? "0" : GetValue<string>("API:MaxMonthsGetAssociationByDevice"));
            
            everCookieExpireSecond = Convert.ToInt64(string.IsNullOrEmpty(GetValue<string>("Redis:EverCookieExpireSecond")) ? "3600" : GetValue<string>("Redis:EverCookieExpireSecond"));
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

        public int RedisDatabase => redisDatabase;

        public long RedisExpired => redisExpired;

        public string JWTPassword => jWTPassword;

        public string JWT => jWT;

        public string LogTrailBaseUrl => logTrailBaseUrl;

        public string SystemName => systemName;

        public bool IsEnableLogTrail => isEnableLogTrail;

        public bool IsExtraSub => isExtraSub;

        public int MaxMonthsGetAssociationByDevice => maxMonthsGetAssociationByDevice;
        
        public long EverCookieExpireSecond => everCookieExpireSecond;

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

        public string JWTIssue => jWTIssue;

        public string JWTAudience => jWTAudience;
    }
}
