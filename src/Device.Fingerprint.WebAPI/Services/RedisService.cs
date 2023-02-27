using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;

namespace Device.Fingerprint.WebAPI.Services
{    
    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }
    }
    public class RedisService: IRedisService
    {        
        private readonly IRedisDatabase redisDatabase;

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
        }

        public IRedisDatabase RedisDatabase => redisDatabase;
    }
}
