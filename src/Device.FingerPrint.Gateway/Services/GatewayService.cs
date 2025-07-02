using Device.FingerPrint.Redis;
using Microsoft.Extensions.Caching.Memory;

namespace Device.FingerPrint.Gateway.Services
{
    public interface IGatewayService
    {
        Task<bool> IsDisableDIAsync();
    }

    public class GatewayService : IGatewayService
    {
        private readonly IRedisService redisService;
        private readonly IMemoryCache memoryCache;
        private const string DisableDICacheKey = "disableDI_cache";

        public GatewayService(IRedisService redisService, IMemoryCache memoryCache)
        {
            this.redisService = redisService;
            this.memoryCache = memoryCache;
        }

        public async Task<bool> IsDisableDIAsync()
        {
            // Try to get from memory cache first
            if (memoryCache.TryGetValue(DisableDICacheKey, out bool cachedValue))
            {
                return cachedValue;
            }

            try
            {
                var disableDIValue = await redisService.GetAsync<string>("disableGateway");
                var result = string.IsNullOrEmpty(disableDIValue) || !disableDIValue.Equals("false", StringComparison.CurrentCultureIgnoreCase);
                
                // Cache the result for 10 seconds
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                };
                memoryCache.Set(DisableDICacheKey, result, cacheOptions);
                
                return result;
            }
            catch 
            { 
                // Cache the default value (true) for 10 seconds in case of error
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                };
                memoryCache.Set(DisableDICacheKey, true, cacheOptions);
            }

            return true;
        }
    }
}