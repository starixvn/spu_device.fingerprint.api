using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }
    }
    public class RedisService : IRedisService
    {
        private readonly IRedisDatabase redisDatabase;

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
        }

        public IRedisDatabase RedisDatabase => redisDatabase;
    }
}
