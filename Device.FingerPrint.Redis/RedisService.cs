namespace Device.FingerPrint.Redis
{
    using StackExchange.Redis;
    using StackExchange.Redis.Extensions.Core.Abstractions;
    using System.Threading.Tasks;

    public interface IRedisService
    {
        IRedisDatabase RedisDatabase { get; }

        Task<T?> HashGetAsync<T>(string hashKey, string key);        

        Task<bool> HashSetAsync<T>(string hashKey, string key, T value);

        Task<bool> HashExistAsync<T>(string hashKey, string key);

        Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt);

        Task<long> HashDeleteAsync(string hashKey, string[] keys);

        Task HashSetAndExpiryAsync<T>(string hashKey, string key, T value, DateTimeOffset expiresAt);
    }

    public class RedisService : IRedisService
    {
        private readonly IRedisDatabase redisDatabase;

        private const bool isSetNotExist = true;

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
        }

        public IRedisDatabase RedisDatabase => redisDatabase;


        public Task<bool> HashSetAsync<T>(string hashKey, string key, T value)
        {
            return redisDatabase.HashSetAsync<T>(hashKey, key, value, nx: isSetNotExist, flag: CommandFlags.FireAndForget);
        }       

        public async Task<T?> HashGetAsync<T>(string hashKey, string key)
        {
            return await redisDatabase.HashGetAsync<T>(hashKey, key);
        }

        public Task<bool> HashExistAsync<T>(string hashKey, string key)
        {
            return redisDatabase.HashExistsAsync(hashKey, key);
        }

        public Task<bool> UpdateExpiryAsync(string key, DateTimeOffset expiresAt)
        {
            return redisDatabase.UpdateExpiryAsync(key, expiresAt, CommandFlags.FireAndForget);
        }

        public Task<long> HashDeleteAsync(string hashKey, string[] keys)
        {
            return redisDatabase.HashDeleteAsync(hashKey, keys);    
        }

        public async Task HashSetAndExpiryAsync<T>(string hashKey, string key, T value, DateTimeOffset expiresAt)
        {
            await HashSetAsync(hashKey, key, value);

            await UpdateExpiryAsync(hashKey, expiresAt);
        }
    }
}
