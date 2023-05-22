using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System.Threading.Tasks;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }

        T GetResult<T>(string RedisKey);

        Task<BrowserDetectModel>  GetBrowserInfo(int SubscriberId, string SessionKey);

        Task<string> GetCookie(int SubscriberId, string SessionKey);

        Task<FingerprintModel> GetFingerPrint(int SubscriberId, string SessionKey);

        Task<HeaderModel> GetHeader(int SubscriberId, string SessionKey);

        Task<HeadlessBrowserModel>  GetHeadless(int SubscriberId, string SessionKey);

        string GetRedisKey(RedisKey Key, int SubscriberId, string SessionKey);

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


        public async Task<BrowserDetectModel>  GetBrowserInfo(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.BrowserInfo, SubscriberId, SessionKey);

            return await RedisDatabase.GetAsync<BrowserDetectModel>(redisKey);
        }

        public async Task<string> GetCookie(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.Cookie, SubscriberId, SessionKey);

            return await RedisDatabase.GetAsync<string>(redisKey);
        }

        public async Task<FingerprintModel> GetFingerPrint(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.FingerPrint, SubscriberId, SessionKey);

            return await RedisDatabase.GetAsync<FingerprintModel>(redisKey);
        }

        public async Task<HeaderModel> GetHeader(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.Header, SubscriberId, SessionKey);

            return await RedisDatabase.GetAsync<HeaderModel>(redisKey);
        }

        public async Task<HeadlessBrowserModel>  GetHeadless(int SubscriberId, string SessionKey)
        {
            var redisKey = GetRedisKey(RedisKey.Headless, SubscriberId, SessionKey);

            return await RedisDatabase.GetAsync<HeadlessBrowserModel>(redisKey);
        }
        
        public string GetRedisKey(RedisKey Key, int SubscriberId, string SessionKey)
        {
            return $"{Key}:{SubscriberId}:{SessionKey}";
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
