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
    }
}
