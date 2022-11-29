using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using StackExchange.Redis.Extensions.Core.Abstractions;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }

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

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
        }

        public IRedisDatabase RedisDatabase => redisDatabase;


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
