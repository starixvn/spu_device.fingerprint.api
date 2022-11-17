using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Services;
using Device.FingerPrint.PortalAPI.Ultilities;
using Fanex.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Device.FingerPrint.PortalAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IRedisService redisService;
        private readonly IEasyNetQService easyNetQService;
        private readonly ITransactionService transactionService;
        private readonly Crypto crypto;
        
        private readonly int redisExpired = 1;


        public TransactionController(
            IAppSettings appSettings,
            IRedisService redisService,
            ITransactionService transactionService,
            IEasyNetQService easyNetQService)
        {
            this.redisService = redisService;
            this.transactionService = transactionService;
            this.easyNetQService = easyNetQService;
            crypto = new Crypto(appSettings.CryptoPassword, appSettings.CryptoIV, appSettings.CryptoSalt);            
        }

        [HttpPost("ce")]
        public void Cookie([FromBody] CookieParameter data)
        {
            if (data != null)
            {
                var redisKey = $"cookie:{data.SubscriberId}:{data.SessionKey}";


                if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                {
                    RedisAddAsync<string>(redisKey, data.Cookie);
                }
                else
                {
                    RedisUpdateExpiryAsync(redisKey);
                }

                AddHeaderToRedis(data.SubscriberId, data.SessionKey);
            }           
        }


        private void AddHeaderToRedis(int SubscriberId, string SessionKey)
        {
            var transactionAttributeService = new TransactionAttributeService();
            var userAgent = Request.Headers["User-Agent"];
            var userAgentInfo = transactionAttributeService.GetUserAgentInfo(userAgent);

            var headerModel = new HeaderModel
            {
                Accept = HttpContext.Request.Headers["Accept"],
                Encoding = HttpContext.Request.Headers["Accept-Encoding"],
                UserAgent = userAgent,
                Headers = String.Join(" ", HttpContext.Request.Headers.Keys),
                BrowserName = userAgentInfo.BrowserName,
                BrowserVersion = userAgentInfo.BrowserVersion,
                OSName = userAgentInfo.OSName
            };

            var redisKey = $"header:{SubscriberId}:{SessionKey}";

            if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
            {
                RedisAddAsync<HeaderModel>(redisKey, headerModel);
            }
            else
            {
                RedisUpdateExpiryAsync(redisKey);
            }
        }

        [HttpPost("hl")]
        public void Headless([FromBody] HeadlessParameter data)
        {
            if (data != null)
            {
                var redisKey = $"headless:{data.SubscriberId}:{data.SessionKey}";

                if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                {
                    RedisAddAsync<HeadlessBrowserModel>(redisKey, data.HeadlessBrowser);
                }
                else
                {
                    RedisUpdateExpiryAsync(redisKey);
                }
            }
        }

        [HttpPost("bi")]
        public void BrowserInfo([FromBody] BrowserDetectParameter data)
        {
            if (data != null)
            {
                var redisKey = $"browserinfo:{data.SubscriberId}:{data.SessionKey}";

                if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                {
                    RedisAddAsync<BrowserDetectModel>(redisKey, data.BrowserDetect);
                }
                else
                {
                    RedisUpdateExpiryAsync(redisKey);
                }
            }

        }

        [HttpPost("fp")]
        public void FingerPrint([FromBody] FingerprintParameter data)
        {
            if (data != null)
            {
                var redisKey = $"fingerprint:{data.SubscriberId}:{data.SessionKey}";

                if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                {
                    RedisAddAsync<FingerprintModel>(redisKey, data.fingerprint);
                }
                else
                {
                    RedisUpdateExpiryAsync(redisKey);
                }
            }
        }

        [HttpPost("transaction")]
        public void Transaction([FromBody] TransactionDTO data)
        {
            // Header không lấy ở đây được
            try
            {
                if (data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SessionKey) && data.SubscriberId > 0)
                {
                    var urlAddress = new Uri(data.Url);
                    var userAgentCode = crypto.GetMD5Hash(data.UserAgent);

                    var transaction = new TransactionModel
                    {
                        SubscriberId = data.SubscriberId,
                        ClientIp = data.ClientIp,
                        LoginName = data.LoginName,
                        SessionKey = data.SessionKey,
                        UserAgent = data.UserAgent,
                        UserAgentCode = userAgentCode,
                        TransTime = DateTime.Now,
                        Url = urlAddress.GetLeftPart(UriPartial.Path),
                        Action = data.Action,
                        TransKey = data.TransKey,
                        CustId = data.CustId,
                        SubAccId = data.SubAccId
                    };

                    var transactionInsertModel = transactionService.MergeData(transaction);

                    easyNetQService.PushData(transactionInsertModel);
                }
            }
            catch (Exception ex)
            {
                LogService.Error("PortalAPI Transaction", ex);
            }
        }


        private void RedisAddAsync<T>(string redisKey, T value)
        {
            redisService.RedisDatabase.AddAsync(redisKey, value, DateTimeOffset.UtcNow.AddMinutes(redisExpired), when: StackExchange.Redis.When.NotExists);
        }

        private void RedisUpdateExpiryAsync(string redisKey)
        {
            redisService.RedisDatabase.UpdateExpiryAsync(redisKey, DateTimeOffset.UtcNow.AddMinutes(redisExpired));
        }
    }
}
