using Device.FingerPrint.Gateway.Model;
using Device.FingerPrint.Gateway.Services;
using Device.FingerPrint.Redis;
using Fanex.Logging;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class ValidateHandler : DelegatingHandler
    {
        private readonly ISubscriberService subscriberService;
        private readonly IRedisService redisService;
        private readonly IMemoryCache memoryCache;

        private static readonly HashSet<string> DebugAccounts = new HashSet<string> { "qctest5oct202019", "qatest05" };
        private const string DisableDICacheKey = "disableDI_cache";

        public ValidateHandler(ISubscriberService subscriberService, IRedisService redisService, IMemoryCache memoryCache)
        {
            this.subscriberService = subscriberService;
            this.redisService = redisService;
            this.memoryCache = memoryCache;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var isDisableDI = await IsDisableDI();
                if (isDisableDI)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }

                if (request != null && request.Content != null && !isDisableDI)
                {
                    var content = await request.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(content);

                        if (transactionRequest != null && !string.IsNullOrWhiteSpace(transactionRequest?.Subscriber) && !string.IsNullOrWhiteSpace(transactionRequest.Account))
                        {
                            if (DebugAccounts.Contains(transactionRequest.Account.ToLower()))
                            {
                                _ = LogService.InfoAsync($"Gateway Debug for Account:{transactionRequest.Account}\n {JsonConvert.SerializeObject(transactionRequest)}");
                            }

                            if (await this.subscriberService.ValidateAsync(transactionRequest?.Subscriber))
                            {
                                transactionRequest.TransDate = DateTime.Now;
                                request.Content = new StringContent(JsonConvert.SerializeObject(transactionRequest), Encoding.UTF8, "application/json");

                                return await base.SendAsync(request, cancellationToken);
                            }
                            else
                            {
                                _ = LogService.InfoAsync($"Gateway Subscriber invalid:\n {JsonConvert.SerializeObject(transactionRequest)}");
                            }
                        }
                        else
                        {
                            _ = LogService.InfoAsync($"Gateway TransactionRequest invalid:\n {JsonConvert.SerializeObject(transactionRequest)}");
                        }
                    }
                    else
                    {
                        _ = LogService.InfoAsync($"Gateway cannot read request.Content:\n {JsonConvert.SerializeObject(request.Content)}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }

        private async Task<bool> IsDisableDI()
        {
            // Try to get from memory cache first
            if (memoryCache.TryGetValue(DisableDICacheKey, out bool cachedValue))
            {
                return cachedValue;
            }

            try
            {
                var disableDIValue = await redisService.GetAsync<string>("disableDI");
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