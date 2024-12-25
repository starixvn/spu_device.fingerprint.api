using System.ComponentModel;

namespace Device.Identification
{
    public interface IAppSettings
    {
        string SPName { get; }

        string SentryDsn { get; }

        string CookieStorage { get; }

        string Storages { get; }

        string ScriptHost { get; }

        string ScriptHostCDN { get; }

        string ScriptVersion { get; }

        DateTime ScriptTime { get; }

        string EncryptPassword { get; }

        string Separator { get; }

        int CachingExpiredSeconds { get; }

        bool IsDisabledDI { get; }

        string FPAPIUrl { get; }

        string FPPortalAPIUrl { get; }

        string RedisDbIndex { get; }

        bool DisableJsChallenge { get; }

        string JsChallengeVersion { get; }

        string JsChallengeAPIUrl { get; }

        string JwtID { get; }

        string JwtSecretKey { get; }

    }

    public class AppSettings : IAppSettings
    {
        private readonly IConfiguration configuration;

        private readonly string spName;

        private readonly string sentryDsn;

        private readonly string cookieStorage;

        private readonly string storages;

        private readonly string scriptHost;

        private readonly string scriptHostCDN;

        private readonly string scriptVersion;

        private readonly DateTime scriptTime;

        private readonly string encryptPassword;

        private readonly string separator;

        private readonly int cachingExpiredSeconds;

        private readonly bool isDisabledDI;

        private readonly string fpAPIUrl;

        private readonly string fpPortalAPIUrl;

        private readonly string redisDbIndex;

        private readonly bool disableJsChallenge;

        private readonly string jsChallengeVersion;

        private readonly string jsChallengeAPIUrl;

        private readonly string jwtID;

        private readonly string jwtSecretKey;

        public AppSettings(IConfiguration configuration)
        {
            this.configuration = configuration;

            spName = GetValue<string>("AppConfig:SPName") ?? string.Empty;

            sentryDsn = GetValue<string>("Sentry:Dsn") ?? string.Empty;

            cookieStorage = GetValue<string>("AppConfig:CookieStorage") ?? string.Empty;

            storages = GetValue<string>("AppConfig:Storages") ?? string.Empty;

            scriptHost = GetValue<string>("AppConfig:ScriptHost") ?? string.Empty;

            scriptHostCDN = GetValue<string>("AppConfig:ScriptHostCDN") ?? string.Empty;

            scriptVersion = GetValue<string>("AppConfig:ScriptVersion") ?? string.Empty;

            scriptTime = Convert.ToDateTime(GetValue<string>("AppConfig:ScriptTime") ?? string.Empty).ToLocalTime();

            encryptPassword = GetValue<string>("AppConfig:EncryptPassword") ?? string.Empty;

            separator = GetValue<string>("AppConfig:Separator") ?? string.Empty;

            cachingExpiredSeconds = Convert.ToInt32(GetValue<string>("AppConfig:CachingExpiredSeconds") ?? "0");

            isDisabledDI = Convert.ToBoolean(GetValue<string>("AppConfig:DisableDIScript") ?? "false");

            fpAPIUrl = GetValue<string>("AppConfig:FP-API-Url") ?? string.Empty;

            fpPortalAPIUrl = GetValue<string>("AppConfig:FP-Portal-API-Url") ?? string.Empty;

            redisDbIndex = GetValue<string>("AppConfig:RedisDbIndex") ?? string.Empty;

            disableJsChallenge = Convert.ToBoolean(GetValue<string>("AppConfig:DisableJsChallenge") ?? "false");

            jsChallengeVersion = GetValue<string>("AppConfig:JsChallengeVersion") ?? string.Empty;

            jsChallengeAPIUrl = GetValue<string>("AppConfig:JsChallengeAPIUrl") ?? string.Empty;

            jwtID = GetValue<string>("AppConfig:JwtID") ?? string.Empty;

            jwtSecretKey = GetValue<string>("AppConfig:JwtSecretKey") ?? string.Empty;
        }

        public string SPName => spName;

        public string SentryDsn => sentryDsn;

        public string CookieStorage => cookieStorage;

        public string Storages => storages;

        public string ScriptHost => scriptHost;

        public string ScriptHostCDN => scriptHostCDN;

        public string ScriptVersion => scriptVersion;

        public DateTime ScriptTime => scriptTime;

        public string EncryptPassword => encryptPassword;

        public string Separator => separator;

        public int CachingExpiredSeconds => cachingExpiredSeconds;

        public bool IsDisabledDI => isDisabledDI;

        public string FPAPIUrl => fpAPIUrl;

        public string FPPortalAPIUrl => fpPortalAPIUrl;

        public string RedisDbIndex => redisDbIndex;

        public bool DisableJsChallenge => disableJsChallenge;

        public string JsChallengeVersion => jsChallengeVersion;

        public string JsChallengeAPIUrl => jsChallengeAPIUrl;

        public string JwtID => jwtID;

        public string JwtSecretKey => jwtSecretKey;

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
