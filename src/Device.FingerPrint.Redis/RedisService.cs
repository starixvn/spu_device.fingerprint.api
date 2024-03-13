namespace Device.FingerPrint.Redis
{
    using StackExchange.Redis;
    using StackExchange.Redis.Extensions.Core.Abstractions;
    using System.Threading.Tasks;

    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }

        Task<T?> HashGetAsync<T>(string hashKey, string key);        

        Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool isForceUpdate);

        Task<bool> HashExistAsync<T>(string hashKey, string key);

        Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt);

        Task<long> HashDeleteAsync(string hashKey, string[] keys);

        Task HashSetAndExpiryAsync<T>(string hashKey, string key, T value, DateTimeOffset expiresAt, bool isForceUpdate = false);

        Task HashSetWithDbIndexAsync(string originService, int redisDatabaseIndex, string hashKey, string fieldKey, string fieldValue, long expireSecond = 0);

        Task<string> HashGetWithDbIndexAsync(int redisDatabaseIndex, string hashKey, string fieldKey);

        Task<T> GetAsync<T>(string key);

        Task<bool> SetAsync<T>(string key, T value);

        Task<bool> AddAsyncAndExpiryAsync<T>(string key, T value, DateTimeOffset expiresAt);

        Task<double> GetTimeSpan(int redisDatabaseIndex, string hashKey);
    }

    public class RedisService : IRedisService
    {
        private readonly IRedisDatabase redisDatabase;
        private readonly IConnectionMultiplexer connectionMultiplexer;

        public RedisService(IRedisClient redisClient, IConnectionMultiplexer connectionMultiplexer)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
            this.connectionMultiplexer = connectionMultiplexer;
        }

        public IRedisDatabase RedisDatabase => redisDatabase;


        public Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool isForceUpdate)
        {
            return redisDatabase.HashSetAsync<T>(hashKey, key, value, nx: !isForceUpdate, flag: CommandFlags.FireAndForget);
        }       

        public async Task<T?> HashGetAsync<T>(string hashKey, string key)
        {
            return await redisDatabase.HashGetAsync<T>(hashKey ?? string.Empty, key);
        }

        public Task<bool> HashExistAsync<T>(string hashKey, string key)
        {
            return redisDatabase.HashExistsAsync(hashKey ?? string.Empty, key);
        }

        public Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt)
        {
            return redisDatabase.UpdateExpiryAsync(key ?? string.Empty, expiresAt, CommandFlags.FireAndForget);
        }

        public Task<long> HashDeleteAsync(string hashKey, string[] keys)
        {
            return redisDatabase.HashDeleteAsync(hashKey ?? string.Empty, keys);    
        }

        public async Task HashSetAndExpiryAsync<T>(string hashKey, string key, T value, DateTimeOffset expiresAt, bool isForceUpdate = false)
        {
            await HashSetAsync(hashKey, key, value, isForceUpdate);

            await UpdateExpiryAsync(hashKey, expiresAt);
        }

        public async Task HashSetWithDbIndexAsync(string originService, int redisDatabaseIndex, string hashKey, string fieldKey, string fieldValue, long expireSecond = 0)
        {
            var db = connectionMultiplexer.GetDatabase(redisDatabaseIndex);

            bool exists = await db.KeyExistsAsync(hashKey);

            await db.HashSetAsync(hashKey, fieldKey, fieldValue);

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

        public async Task<T> GetAsync<T>(string key)
        {
            return await redisDatabase.GetAsync<T>(key);
        }

        public Task<bool> SetAsync<T>(string key, T value)
        {
            return redisDatabase.SetAddAsync<T>(key, value, flag: CommandFlags.FireAndForget);
        }

        public Task<bool> AddAsyncAndExpiryAsync<T>(string key, T value, DateTimeOffset expiresAt)
        {
            return redisDatabase.AddAsync(key, value, expiresAt, flag: CommandFlags.FireAndForget);
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
