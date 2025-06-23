namespace Device.FingerPrint.Gateway.Services
{
    using Device.FingerPrint.Gateway.Model;
    using Device.FingerPrint.Redis;
    using System.Text.Json;

    public interface ISubscriberService
    {
        Task<bool> ValidateAsync(string subscriber);
    }

    public class SubscriberService : ISubscriberService
    {
        private readonly IAppSettings appSettings;
        private readonly IRedisService redisService;

        public SubscriberService(IAppSettings appSettings, IRedisService redisService)
        {
            this.appSettings = appSettings;
            this.redisService = redisService;
        }

        public async Task<bool> ValidateAsync(string subscriber)
        {

            if (!string.IsNullOrWhiteSpace(subscriber))
            {
                var subscriberInfo = await GetSubscriberInfoAsync(subscriber);

                if (subscriberInfo != null && subscriberInfo.SubscriberId != 0)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<SubscriberModel> GetSubscriberInfoAsync(string subName)
        {

            if (!string.IsNullOrWhiteSpace(subName))
            {
                if (IsExtraSubscriber(subName))
                {
                    return GetExtraSubscriberInfo(subName);
                }
                else
                {
                    return await GetSubscriberInfo(subName);
                }  
            }

            return new SubscriberModel();
        }

        private async Task<SubscriberModel> GetSubscriberInfoByCache(string cacheKey, string subName)
        {
            try
            {
                return await redisService.GetAsync<SubscriberModel>(cacheKey);
            }
            catch (Exception)
            {
                return await GetDataSubscriberAndSetCacheAsync(subName, cacheKey);
            }
        }

        private SubscriberModel GetExtraSubscriberInfo(string subName)
        {
            if (!string.IsNullOrWhiteSpace(subName))
            {
                return appSettings.ExtraSubscribers.FirstOrDefault(x => x.SubscriberName.Equals(subName, StringComparison.OrdinalIgnoreCase));
            }

            return new SubscriberModel();
        }

        private async Task<SubscriberModel> GetSubscriberInfo(string subName)
        {
            if(!string.IsNullOrWhiteSpace(subName))
            {
                var cacheKey = $"GatewayAPI:{subName}";

                var subData = await GetSubscriberInfoByCache(cacheKey, subName);

                if(subData != null)
                {
                    return subData;
                }
                else
                {
                    subData = await GetDataSubscriberAsync(subName);

                    if(subData != null && subData.SubscriberId != 0)
                    {
                        _ = redisService.AddAsyncAndExpiryAsync(cacheKey, subData, DateTimeOffset.UtcNow.AddMinutes(120));

                        return subData;
                    }                    
                }
            }

            return new SubscriberModel();
        }

        private async Task<SubscriberModel> GetDataSubscriberAndSetCacheAsync(string subName, string cacheKey)
        {
            var subData = await GetDataSubscriberAsync(subName);

            if (subData != null && subData.SubscriberId != 0)
            {
                _ = redisService.AddAsyncAndExpiryAsync(cacheKey, subData, DateTimeOffset.UtcNow.AddMinutes(120));

                return subData;
            }

            return new SubscriberModel();
        }

        private async Task<SubscriberModel> GetDataSubscriberAsync(string subName)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromMinutes(10);

                var result = await httpClient.GetAsync($"{appSettings.GetSubscriberInfo}?subscriberName={subName}")
                    .ContinueWith((responseTask) =>
                    {
                        var response = responseTask.Result;
                        var jsonString = response.Content.ReadAsStringAsync();

                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        return JsonSerializer.Deserialize<SubscriberModel>(jsonString.Result, options);
                    });

                return result ?? new SubscriberModel();
            }
        }

        private bool IsExtraSubscriber(string subscriberName)
        {
            if (appSettings.ExtraSubscribers != null && appSettings.ExtraSubscribers.Any())
            {
                return appSettings.ExtraSubscribers.Exists(x => x.SubscriberName.ToLower() == subscriberName.ToLower());
            }

            return false;
        }
    }
}
