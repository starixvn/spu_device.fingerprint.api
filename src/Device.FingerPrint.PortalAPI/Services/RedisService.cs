using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }

        IRedisDatabase CookieDatabase { get; }

        IRedisDatabase HeadlessDatabase { get; }

        IRedisDatabase BrowserDetectDatabase { get; }

        IRedisDatabase FingerPrintDatabase { get; }

        T GetResult<T>(string RedisKey);

        BrowserDetectModel GetBrowserInfo(int SubscriberId, string SessionKey);

        string GetCookie(int SubscriberId, string SessionKey);

        FingerprintModel GetFingerPrint(int SubscriberId, string SessionKey);

        HeaderModel GetHeader(int SubscriberId, string SessionKey);

        HeadlessBrowserModel GetHeadless(int SubscriberId, string SessionKey);

        string GetRedisKey(string Key, int SubscriberId, string SessionKey);
    }

    public class RedisService : IRedisService
    {
        private readonly IRedisDatabase redisDatabase;

        private readonly IRedisDatabase cookieDatabase;

        private readonly IRedisDatabase headlessDatabase;

        private readonly IRedisDatabase browserDetectDatabase;

        private readonly IRedisDatabase fingerPrintDatabase;

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();

            cookieDatabase = redisClient.Db14;

            headlessDatabase = redisClient.Db15;

            browserDetectDatabase = redisClient.Db13;

            fingerPrintDatabase = redisClient.Db12;
        }

        public IRedisDatabase RedisDatabase => redisDatabase;

        public IRedisDatabase CookieDatabase => cookieDatabase;

        public IRedisDatabase HeadlessDatabase => headlessDatabase;

        public IRedisDatabase BrowserDetectDatabase => browserDetectDatabase;

        public IRedisDatabase FingerPrintDatabase => fingerPrintDatabase;


        public BrowserDetectModel GetBrowserInfo(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.BrowserInfo.ToString(), SubscriberId, SessionKey);
            return RedisDatabase.GetAsync<BrowserDetectModel>(redisKey).Result;
        }

        public string GetCookie(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.Cookie.ToString(), SubscriberId, SessionKey);
            return RedisDatabase.GetAsync<string>(redisKey).Result;
        }

        public FingerprintModel GetFingerPrint(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.FingerPrint.ToString(), SubscriberId, SessionKey);
            return RedisDatabase.GetAsync<FingerprintModel>(redisKey).Result;
        }

        public HeaderModel GetHeader(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.Header.ToString(), SubscriberId, SessionKey);
            return RedisDatabase.GetAsync<HeaderModel>(redisKey).Result;
        }

        public HeadlessBrowserModel GetHeadless(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.Headless.ToString(), SubscriberId, SessionKey);
            return RedisDatabase.GetAsync<HeadlessBrowserModel>(redisKey).Result;
        }

        public string GetRedisKey(string Key, int SubscriberId, string SessionKey)
        {
            return $"{Key}:{SubscriberId}:{SessionKey}";
        }

        public T GetResult<T>(string RedisKey)
        {
            return RedisDatabase.GetAsync<T>(RedisKey).Result;
        }
    }
}
