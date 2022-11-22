using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Services;
using Device.FingerPrint.PortalAPI.Ultilities;
using Fanex.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

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

        private readonly int redisExpired = 10;


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
                var redisKey = GetRedisKey("cookie", data.SubscriberId, data.SessionKey);

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
            var headerModel = new HeaderModel
            {
                Accept = HttpContext.Request.Headers["Accept"],
                Encoding = HttpContext.Request.Headers["Accept-Encoding"],
                UserAgent = Request.Headers["User-Agent"],
                Headers = String.Join(" ", HttpContext.Request.Headers.Keys),
                secChUa = Request.Headers["sec-ch-ua"].Count > 0 ? Request.Headers["sec-ch-ua"].ToString() : "Other",
                secChUaPlatform = Request.Headers["sec-ch-ua-platform"].Count > 0 ? Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "") : "",
                secChUaMobile = Request.Headers["sec-ch-ua-mobile"].Count > 0 ? Request.Headers["sec-ch-ua-mobile"].ToString() : ""
            };

            var redisKey = GetRedisKey("header", SubscriberId, SessionKey);

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
                var redisKey = GetRedisKey("headless", data.SubscriberId, data.SessionKey);

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
                var redisKey = GetRedisKey("browserinfo", data.SubscriberId, data.SessionKey);

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
                var redisKey = GetRedisKey("fingerprint", data.SubscriberId, data.SessionKey);

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

        [HttpPost("removeRedis")]
        public void RemoveRedis([FromForm] int SubscriberId, [FromForm] string SessionKey)
        {
            RedisRemoveAll(SubscriberId, SessionKey);
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
                        UserAgent = data.UserAgent,
                        UserAgentCode = userAgentCode,
                        TransTime = DateTime.Now,
                        Url = urlAddress.GetLeftPart(UriPartial.Path),
                        Action = data.Action,
                        TransKey = data.TransKey,
                        CustId = data.CustId,
                        SubAccId = data.SubAccId
                    };

                    var transactionInsertModel = transactionService.MergeData(transaction, data.SessionKey);

                    easyNetQService.PushData(transactionInsertModel);

                    RedisRemoveAll(data.SubscriberId, data.SessionKey);
                }
            }
            catch (Exception ex)
            {
                LogService.Error("PortalAPI Transaction", ex);
            }
        }


        private string GetRedisKey(string key, int subscriberId, string sessionKey)
        {
            return $"{key}:{subscriberId}:{sessionKey}";
        }

        private void RedisAddAsync<T>(string redisKey, T value)
        {
            redisService.RedisDatabase.AddAsync(redisKey, value, DateTimeOffset.UtcNow.AddMinutes(redisExpired), when: StackExchange.Redis.When.NotExists);
        }

        private void RedisUpdateExpiryAsync(string redisKey)
        {
            redisService.RedisDatabase.UpdateExpiryAsync(redisKey, DateTimeOffset.UtcNow.AddMinutes(redisExpired));
        }

        private void RedisRemoveAll(int subscriberId, string sessionKey)
        {
            var arrKey = new List<string> { "cookie", "header", "headless", "browserinfo", "fingerprint" };
            var arrRedisKey = new List<string>();
            foreach (var key in arrKey)
            {
                arrRedisKey.Add(GetRedisKey(key, subscriberId, sessionKey));
            }

            redisService.RedisDatabase.RemoveAllAsync(arrRedisKey.ToArray());
        }
    }
}
