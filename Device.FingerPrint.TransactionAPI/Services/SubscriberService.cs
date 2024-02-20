namespace Device.FingerPrint.TransactionAPI.Services
{
    using System.Text.Json;
    using Device.FingerPrint.Redis;
    using Device.FingerPrint.TransactionAPI.Model;

    public interface ISubscriberService
    {
        Task<string> ValidateAsync(TransactionRequest transactionRequest, bool isExtraSubscriber);
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

        public async Task<string> ValidateAsync(TransactionRequest transactionRequest, bool isExtraSubscriber)

        {
            if (string.IsNullOrEmpty(transactionRequest.Account))
            {
                return TransactionResult.EmptyAccount;
            }

            if (!string.IsNullOrWhiteSpace(transactionRequest.Subscriber))
            {
                var subscriber = await GetSubscriberInfoAsync(transactionRequest.Subscriber, isExtraSubscriber, appSettings.ExtraSubscribers);

                if (subscriber != null && subscriber.SubscriberId != 0)
                {
                    transactionRequest.SubscriberId = subscriber.SubscriberId;

                    return TransactionResult.Success;
                }
            }

            return TransactionResult.NotExistSubscriber;
        }

        private async Task<SubscriberModel> GetSubscriberInfoAsync(string subName, bool isExtraSubscriber, List<SubscriberModel> listExtraSubscribersModel)
        {

            if (!string.IsNullOrWhiteSpace(subName))
            {
                var cacheKey = $"WebAPI:{subName}";

                SubscriberModel subInfo = await GetSubscriberInfoByCache(cacheKey, subName);

                if (subInfo != null && subInfo.SubscriberId != 0)
                {
                    return subInfo;
                }

                if (isExtraSubscriber)
                {
                    return GetExtraSubscriberInfo(subName, listExtraSubscribersModel);
                }

                return await GetDataSubscriberAndSetCacheAsync(subName, cacheKey);

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

        private SubscriberModel GetExtraSubscriberInfo(string subName, List<SubscriberModel> listExtraSubscribersModel)
        {
            if (!string.IsNullOrWhiteSpace(subName))
            {
                var cacheKey = $"WebAPI:{subName}";

                var subData = listExtraSubscribersModel.FirstOrDefault(x => x.SubscriberName.Equals(subName, StringComparison.OrdinalIgnoreCase));

                if (subData != null && subData.SubscriberId != 0)
                {
                    _ = redisService.AddAsyncAndExpiryAsync(cacheKey, subData, DateTimeOffset.UtcNow.AddMinutes(120));

                    return subData;
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
            }

            return subData;
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

                        return JsonSerializer.Deserialize<SubscriberModel>(jsonString.Result);
                    });

                return result;
            }
        }
    }
}
