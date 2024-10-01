using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Implementations;

namespace Device.FingerPrint.Redis
{
    public interface IRedisMultiplexService
    {
        Task HashSetWithDbIndexAsync(string originService, int redisDatabaseIndex, string hashKey, string fieldKey, string fieldValue, long expireSecond);

        Task HashSetMultipleFieldsWithDbIndexAsync(string originService, int redisDatabaseIndex, string hashKey, Dictionary<string, string> fieldKeyValuePairs, long expireSecond);

        Task<string> HashGetWithDbIndexAsync(int redisDatabaseIndex, string hashKey, string fieldKey);

        Task<double> GetTimeSpan(int redisDatabaseIndex, string hashKey);

        IDatabase GetRedisEverTaggingDb();

        Task EverTagging_AddItemWithExpiryAsync(string hashKey, string itemKey, string value, TimeSpan expiry);

        Task<string> EverTagging_GetValueFromRedis(string value);

        Task<string> EverTagging_GetValuesFromRedis(IEnumerable<string> values);

        Task<bool> EverTagging_DeleteValueFromRedis(string key);
    }

    public class RedisMultiplexService : IRedisMultiplexService
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;

        private readonly IDatabase redisEverTaggingDb;

        public RedisMultiplexService(IConnectionMultiplexer connectionMultiplexer, int EverTaggingDbIndex)
        {
            this.connectionMultiplexer = connectionMultiplexer;
            redisEverTaggingDb = connectionMultiplexer.GetDatabase(EverTaggingDbIndex);
        }

        public IDatabase GetRedisEverTaggingDb()
        {
            return redisEverTaggingDb;
        }

        public async Task HashSetMultipleFieldsWithDbIndexAsync(string originService, int redisDatabaseIndex, string hashKey, Dictionary<string, string> fieldKeyValuePairs, long expireSecond = 0)
        {
            var db = connectionMultiplexer.GetDatabase(redisDatabaseIndex);

            bool exists = await db.KeyExistsAsync(hashKey);

            foreach (var pair in fieldKeyValuePairs)
            {
                await db.HashSetAsync(hashKey, pair.Key, pair.Value);
            }

            if (!exists && expireSecond > 0)
            {
                await db.HashSetAsync(hashKey, "CreatedTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), When.NotExists, CommandFlags.FireAndForget);
                await db.HashSetAsync(hashKey, "OriginService", originService, When.NotExists, CommandFlags.FireAndForget);

                await db.KeyExpireAsync(hashKey, TimeSpan.FromSeconds(expireSecond));
            }
        }

        public async Task HashSetWithDbIndexAsync(string originService, int redisDatabaseIndex, string hashKey, string fieldKey, string fieldValue, long expireSecond = 0)
        {
            var db = connectionMultiplexer.GetDatabase(redisDatabaseIndex);

            bool exists = await db.KeyExistsAsync(hashKey);

            await db.HashSetAsync(hashKey, fieldKey, fieldValue, When.NotExists, CommandFlags.FireAndForget);

            if (!exists && expireSecond > 0)
            {
                await db.HashSetAsync(hashKey, "CreatedTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"), When.NotExists, CommandFlags.FireAndForget);
                await db.HashSetAsync(hashKey, "OriginService", originService, When.NotExists, CommandFlags.FireAndForget);

                await db.KeyExpireAsync(hashKey, TimeSpan.FromSeconds(expireSecond));
            }
        }

        public async Task<string> HashGetWithDbIndexAsync(int redisDatabaseIndex, string hashKey, string fieldKey)
        {
            var db = connectionMultiplexer.GetDatabase(redisDatabaseIndex);

            return await db.HashGetAsync(hashKey, fieldKey);
        }

        public async Task<double> GetTimeSpan(int redisDatabaseIndex, string hashKey)
        {
            var db = connectionMultiplexer.GetDatabase(redisDatabaseIndex);
            var timeToLive = await db.KeyTimeToLiveAsync(hashKey);

            if (timeToLive != null && timeToLive.HasValue)
            {
                return timeToLive.Value.TotalSeconds;
            }

            return -1;
        }

        public async Task EverTagging_AddItemWithExpiryAsync(string hashKey, string itemKey, string value, TimeSpan expiry)
        {
            string expiryKey = $"{hashKey}:{itemKey}";
            await redisEverTaggingDb.StringSetAsync(expiryKey, value);
            await redisEverTaggingDb.KeyExpireAsync(expiryKey, expiry);
        }

        public async Task<string> EverTagging_GetValueFromRedis(string value)
        {
            var result = await redisEverTaggingDb.StringGetAsync(value);
            if (result.HasValue)
            {
                return result.ToString();
            }

            return string.Empty;
        }

        public async Task<string> EverTagging_GetValuesFromRedis(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                var result = await redisEverTaggingDb.StringGetAsync(value);
                if (result.HasValue)
                {
                    return value;
                }
            }
            return string.Empty;
        }

        public async Task<bool> EverTagging_DeleteValueFromRedis(string key)
        {
            var result = await redisEverTaggingDb.StringGetDeleteAsync(key);
            if (!result.IsNull)
            {
                return true;
            }

            return false;
        }
    }
}
