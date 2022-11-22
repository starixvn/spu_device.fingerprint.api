using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Services;
using Device.FingerPrint.PortalAPI.Ultilities;
using Fanex.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sentry;
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
        private readonly ISentryClient sentryClient;
        private readonly int redisExpired = 10;
        enum RedisKey
        {
            Cookie,
            Headless,
            FingerPrint,
            BrowserInfo,
            Header
        }

        public TransactionController(
            IAppSettings appSettings,
            IRedisService redisService,
            ITransactionService transactionService,
            IEasyNetQService easyNetQService,
             ISentryClient sentryClient)
        {
            this.redisService = redisService;
            this.transactionService = transactionService;
            this.easyNetQService = easyNetQService;
            crypto = new Crypto(appSettings.CryptoPassword, appSettings.CryptoIV, appSettings.CryptoSalt);
            this.sentryClient = sentryClient;
        }

        [HttpPost("ce")]
        public void Cookie([FromBody] string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                try
                {
                    var descryptData = crypto.Decrypt(data);

                    if (!string.IsNullOrWhiteSpace(descryptData))
                    {
                        var deserilizeData = JsonConvert.DeserializeObject<CookieParameter>(descryptData);

                        var redisKey = GetRedisKey(RedisKey.Cookie.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                        if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                        {
                            RedisAddAsync(redisKey, deserilizeData.Cookie);
                        }
                        else
                        {
                            RedisUpdateExpiryAsync(redisKey);
                        }

                        AddHeaderToRedis(deserilizeData.SubscriberId, deserilizeData.SessionKey);
                    }
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }
            }
        }

        [HttpPost("hl")]
        public void Headless([FromBody] string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                try
                {
                    var descryptData = crypto.Decrypt(data);

                    if (!string.IsNullOrWhiteSpace(descryptData))
                    {
                        var deserilizeData = JsonConvert.DeserializeObject<HeadlessParameter>(descryptData);

                        var redisKey = GetRedisKey(RedisKey.Headless.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                        if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                        {
                            RedisAddAsync<HeadlessBrowserModel>(redisKey, deserilizeData.HeadlessBrowser);
                        }
                        else
                        {
                            RedisUpdateExpiryAsync(redisKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }
            }
        }

        [HttpPost("bi")]
        public void BrowserInfo([FromBody] string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {

                try
                {
                    var descryptData = crypto.Decrypt(data);

                    if (!string.IsNullOrWhiteSpace(descryptData))
                    {
                        var deserilizeData = JsonConvert.DeserializeObject<BrowserDetectParameter>(descryptData);

                        var redisKey = GetRedisKey(RedisKey.BrowserInfo.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                        if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                        {
                            RedisAddAsync<BrowserDetectModel>(redisKey, deserilizeData.BrowserDetect);
                        }
                        else
                        {
                            RedisUpdateExpiryAsync(redisKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }
            }
        }

        [HttpPost("fp")]
        public void FingerPrint([FromBody] string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {

                try
                {
                    var descryptData = crypto.Decrypt(data);

                    if (!string.IsNullOrWhiteSpace(descryptData))
                    {
                        var deserilizeData = JsonConvert.DeserializeObject<FingerprintParameter>(descryptData);

                        var redisKey = GetRedisKey(RedisKey.FingerPrint.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                        if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                        {
                            RedisAddAsync<FingerprintModel>(redisKey, deserilizeData.fingerprint);
                        }
                        else
                        {
                            RedisUpdateExpiryAsync(redisKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
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
                sentryClient.CaptureException(ex);
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
            var arrRedisKey = new List<string>();

            foreach (var key in Enum.GetNames(typeof(RedisKey)))
            {
                var redisKey = GetRedisKey(key, subscriberId, sessionKey);

                arrRedisKey.Add(redisKey);
            }

            redisService.RedisDatabase.RemoveAllAsync(arrRedisKey.ToArray());
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

            var redisKey = GetRedisKey(RedisKey.Header.ToString(), SubscriberId, SessionKey);

            if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
            {
                RedisAddAsync(redisKey, headerModel);
            }
            else
            {
                RedisUpdateExpiryAsync(redisKey);
            }
        }

    }
}
