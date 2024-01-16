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
    }

    public class RedisService : IRedisService
    {
        private readonly IRedisDatabase redisDatabase;

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
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
    }
}
