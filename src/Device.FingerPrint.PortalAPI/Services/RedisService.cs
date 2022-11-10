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

        IRedisDatabase CookieDatabase { get; }

        IRedisDatabase HeadlessDatabase { get; }
    }
    public class RedisService : IRedisService
    {
        private readonly IRedisDatabase redisDatabase;

        private readonly IRedisDatabase cookieDatabase;

        private readonly IRedisDatabase headlessDatabase;

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();

            cookieDatabase = redisClient.Db14;

            headlessDatabase = redisClient.Db15;
        }

        public IRedisDatabase RedisDatabase => redisDatabase;

        public IRedisDatabase CookieDatabase => cookieDatabase;

        public IRedisDatabase HeadlessDatabase => headlessDatabase;
    }
}
