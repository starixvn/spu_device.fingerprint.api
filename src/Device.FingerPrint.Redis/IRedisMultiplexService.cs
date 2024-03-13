using StackExchange.Redis;

namespace Device.FingerPrint.Redis
{
    public interface IRedisMultiplexService
    {
        public Task HashSetWithDbIndexAsync(string originService, int redisDatabaseIndex, string hashKey, string fieldKey, string fieldValue, long expireSecond);

        public Task<string> HashGetWithDbIndexAsync(int redisDatabaseIndex, string hashKey, string fieldKey);

        public Task<double> GetTimeSpan(int redisDatabaseIndex, string hashKey);
    }

    public class RedisMultiplexService : IRedisMultiplexService
    {
        private readonly IConnectionMultiplexer connectionMultiplexer;

        public RedisMultiplexService(IConnectionMultiplexer connectionMultiplexer)
        {
            this.connectionMultiplexer = connectionMultiplexer;
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
    }
}
