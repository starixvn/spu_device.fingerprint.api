using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Services;
using Device.FingerPrint.PortalAPI.Ultilities;
using Fanex.Logging;
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
        private readonly Crypto crypto;

        public TransactionController(
            IAppSettings appSettings,
            IRedisService redisService,
            IEasyNetQService easyNetQService)
        {
            this.redisService = redisService;
            this.easyNetQService = easyNetQService;
            crypto = new Crypto(appSettings.CryptoPassword, appSettings.CryptoIV, appSettings.CryptoSalt);
        }

        [HttpPost("ce")]
        public void Cookie([FromBody] CookieParameter data)
        {
            if (data != null)
            {
                var redisKey = $"cookie:{data.SubscriberId}:{data.RedisKey}";

                if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                {
                    redisService.RedisDatabase.AddAsync(redisKey, data.Cookie, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
                }
            }
        }

        [HttpPost("hl")]
        public void Headless([FromBody] HeadlessParameter data)
        {
            if (data != null)
            {
                var redisKey = $"headless:{data.SubscriberId}:{data.RedisKey}";

                if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                {
                    redisService.RedisDatabase.AddAsync(redisKey, data.HeadlessBrowser, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
                }
            }

        }

        [HttpPost("bi")]
        public void BrowserInfo([FromBody] BrowserDetectParameter data)
        {
            if (data != null)
            {
                var redisKey = $"browserinfo:{data.SubscriberId}:{data.RedisKey}";

                if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                {
                    redisService.RedisDatabase.AddAsync(redisKey, data.BrowserDetect, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
                }
            }

        }

        [HttpPost("fp")]
        public void FingerPrint([FromBody] FingerprintParameter data)
        {
            if (data != null)
            {
                var redisKey = $"fingerprint:{data.SubscriberId}:{data.RedisKey}";

                if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                {
                    redisService.RedisDatabase.AddAsync(redisKey, data.fingerprint, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
                }
            }
        }

        [HttpGet("getcookie")]
        public ClientTransactionCookieModel GetCookie(string sessionKey)
        {
            return redisService.RedisDatabase.GetAsync<ClientTransactionCookieModel>(sessionKey).Result;
        }

        [HttpPost("transaction")]
        public void Transaction([FromBody] TransactionDTO data)
        {
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

                    easyNetQService.PushData(transaction);
                }
            }
            catch (Exception ex)
            {
                LogService.Error("PortalAPI Transaction", ex);
            }
        }
    }
}
