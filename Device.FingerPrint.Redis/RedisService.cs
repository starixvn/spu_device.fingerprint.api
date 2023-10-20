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

        Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresAt);

        Task<long> HashDeleteAsync(string hashKey, string[] keys);

        Task<bool> HashSetAndExpiryAsync<T>(string hashKey, string key, T value, TimeSpan expiresAt);
    }

    public class RedisService : IRedisService
    {
        private readonly IRedisDatabase redisDatabase;

        public RedisService(IRedisClient redisClient)
        {
            redisDatabase = redisClient.GetDefaultDatabase();
        }

        public IRedisDatabase RedisDatabase => redisDatabase;

        public Task<bool> HashSetAsync<T>(string hashKey, string key, T value)
        {
            return redisDatabase.HashSetAsync<T>(hashKey, key, value, flag: CommandFlags.FireAndForget);
        }       

        public Task<T?> HashGetAsync<T>(string hashKey, string key)
        {
            return redisDatabase.HashGetAsync<T>(hashKey, key);
        }

        public Task<bool> HashExistAsync<T>(string hashKey, string key)
        {
            return redisDatabase.HashExistsAsync(hashKey, key);
        }

        public Task<bool> UpdateExpiryAsync(string key, TimeSpan expiresAt)
        {
            return redisDatabase.UpdateExpiryAsync(key, expiresAt, CommandFlags.FireAndForget);
        }

        public Task<long> HashDeleteAsync(string hashKey, string[] keys)
        {
            return redisDatabase.HashDeleteAsync(hashKey, keys);    
        }

        public Task<bool> HashSetAndExpiryAsync<T>(string hashKey, string key, T value, TimeSpan expiresAt)
        {
            var result = redisDatabase.HashSetAsync(hashKey, key, value);

            if (result.IsCompleted)
            {                
                return UpdateExpiryAsync(hashKey, expiresAt);
            }

            return result;
        }
    }
}
