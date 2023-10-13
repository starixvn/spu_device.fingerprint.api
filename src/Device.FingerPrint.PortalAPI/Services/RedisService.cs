namespace Device.FingerPrint.PortalAPI.Services
{
    using Device.FingerPrint.PortalAPI.Models;
    using StackExchange.Redis.Extensions.Core.Abstractions;
    using System;
    using System.Threading.Tasks;
    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }

        Task<T> HashGetAsync<T>(RedisKey redisKey, int subscriberId, string sessionKey);

        string GetHashKey(int subscriberId, string sessionKey);
    }

    public class RedisService : IRedisService
    {
        private readonly IRedisDatabase redisDatabase;

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
        }

        public IRedisDatabase RedisDatabase => redisDatabase;


        public async Task<T> HashGetAsync<T>(RedisKey redisKey, int subscriberId, string sessionKey)
        {
            var hashKey = GetHashKey(subscriberId, sessionKey);
            var key = Enum.GetName(typeof(RedisKey), redisKey);

            return await RedisDatabase.HashGetAsync<T>(hashKey, key);
        }

        public string GetHashKey(int subscriberId, string sessionKey)
        {
            return $"{subscriberId}:{sessionKey}";
        }
    }
}
