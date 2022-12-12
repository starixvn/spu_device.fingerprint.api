using Device.FingerPrint.Core;
using Device.FingerPrint.Features.ClientTransaction.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Services;
using Device.FingerPrint.PortalAPI.Ultilities;
using Fanex.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.PortalAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IRedisService redisService;
        private readonly IEasyNetQService easyNetQService;
        private readonly ITransactionService transactionService;
        private readonly ISubscriberServices subscriberServices;
        private readonly IClientTransactionService clientTransactionService;
        private readonly Crypto crypto;
        private readonly ILogger _logger;

        private readonly int redisExpired = 10;
        private readonly int redisExpired = 10;

        public TransactionController(
            IAppSettings appSettings,
            IRedisService redisService,
            ITransactionService transactionService,
            IEasyNetQService easyNetQService,
            ISubscriberServices subscriberServices,
            IClientTransactionService clientTransactionService,
            ILogger logger)
        {
            this.redisService = redisService;
            this.transactionService = transactionService;
            this.easyNetQService = easyNetQService;
            this.subscriberServices = subscriberServices;
            this.clientTransactionService = clientTransactionService;
            crypto = new Crypto(appSettings.CryptoPassword, appSettings.CryptoIV, appSettings.CryptoSalt);
            _logger = logger;
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ce")]
        public void Cookie([FromBody] string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var descryptData = crypto.Decrypt(data);

                if (!string.IsNullOrWhiteSpace(descryptData))
                {
                    var deserilizeData = JsonConvert.DeserializeObject<CookieParameter>(descryptData);

                    var redisKey = redisService.GetRedisKey(RedisKey.Cookie.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                    if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                    {
                        RedisAddAsync(redisKey, deserilizeData.Cookie);
                    }
                    else
                    {
                        RedisReplaceAsync(redisKey, deserilizeData.Cookie);
                    }

                    AddHeaderToRedis(deserilizeData.SubscriberId, deserilizeData.SessionKey);
                }
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("hl")]
        public void Headless([FromBody] string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var descryptData = crypto.Decrypt(data);

                if (!string.IsNullOrWhiteSpace(descryptData))
                {
                    var deserilizeData = JsonConvert.DeserializeObject<HeadlessParameter>(descryptData);

                    var redisKey = redisService.GetRedisKey(RedisKey.Headless.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                    if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                    {
                        RedisAddAsync(redisKey, deserilizeData.HeadlessBrowser);
                    }
                    else
                    {
                        RedisReplaceAsync(redisKey, deserilizeData.HeadlessBrowser);
                    }
                }
        public void Headless([FromBody] string data)
            if (!string.IsNullOrWhiteSpace(data))
                try
                {
                    var descryptData = crypto.Decrypt(data);

                    if (!string.IsNullOrWhiteSpace(descryptData))
                    {
                        var deserilizeData = JsonConvert.DeserializeObject<HeadlessParameter>(descryptData);

                        var redisKey = redisService.GetRedisKey(RedisKey.Headless.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);
                        if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                        {
                            RedisAddAsync<HeadlessBrowserModel>(redisKey, deserilizeData.HeadlessBrowser);
                }
                else
                {
                    RedisUpdateExpiryAsync(redisKey);
                        }
                        else
                        {
                            RedisUpdateExpiryAsync(redisKey);
                        }
                    }
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("bi")]
        public void BrowserInfo([FromBody] string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var descryptData = crypto.Decrypt(data);

                if (!string.IsNullOrWhiteSpace(descryptData))
                {
                    var deserilizeData = JsonConvert.DeserializeObject<BrowserDetectParameter>(descryptData);

                    var redisKey = redisService.GetRedisKey(RedisKey.BrowserInfo.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                    if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                    {
                        RedisAddAsync(redisKey, deserilizeData.BrowserDetect);
                    }
                    else
                    {
                        RedisReplaceAsync(redisKey, deserilizeData.BrowserDetect);
                    }
                }
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("fp")]
        public void FingerPrint([FromBody] string data)
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var descryptData = crypto.Decrypt(data);

                if (!string.IsNullOrWhiteSpace(descryptData))
                {
                    var deserilizeData = JsonConvert.DeserializeObject<FingerprintParameter>(descryptData);

                    var redisKey = redisService.GetRedisKey(RedisKey.FingerPrint.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                    if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                    {
                        RedisAddAsync(redisKey, deserilizeData.fingerprint);
                    }
                    else
                    {
                        RedisReplaceAsync(redisKey, deserilizeData.fingerprint);
                    }
                }
        [HttpPost("bi")]
        public void BrowserInfo([FromBody] string data)
            if (!string.IsNullOrWhiteSpace(data))
            {

                try
                {
                    var descryptData = crypto.Decrypt(data);

                    if (!string.IsNullOrWhiteSpace(descryptData))
                    {
                        var deserilizeData = JsonConvert.DeserializeObject<BrowserDetectParameter>(descryptData);

                        var redisKey = redisService.GetRedisKey(RedisKey.BrowserInfo.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

                        if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                        {
                            RedisAddAsync<BrowserDetectModel>(redisKey, deserilizeData.BrowserDetect);
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

                        var redisKey = redisService.GetRedisKey(RedisKey.FingerPrint.ToString(), deserilizeData.SubscriberId, deserilizeData.SessionKey);

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
        }

        [HttpPost("transaction")]
        public void Transaction([FromBody] TransactionDTO data)
        {
            if (data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SessionKey) && !string.IsNullOrWhiteSpace(data.SubscriberName))
            {
                var subscriber = subscriberServices.GetSubscriberByName(data.SubscriberName);
                if (subscriber != null)
                {
                    var urlAddress = new Uri(data.Url);
                    var userAgentCode = crypto.GetMD5Hash(data.UserAgent);

                    var transaction = new TransactionModel
                    {
                        SubscriberId = subscriber.SubscriberId,
                        ClientIP = data.ClientIp,
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

                    var browserInfo = redisService.GetBrowserInfo(subscriber.SubscriberId, data.SessionKey);
                    var cookie = redisService.GetCookie(subscriber.SubscriberId, data.SessionKey);
                    var fingerPrint = redisService.GetFingerPrint(subscriber.SubscriberId, data.SessionKey);
                    var header = redisService.GetHeader(subscriber.SubscriberId, data.SessionKey);
                    var headless = redisService.GetHeadless(subscriber.SubscriberId, data.SessionKey);

                    var transactionInsertModel = transactionService.MergeData(transaction, browserInfo, cookie, fingerPrint, header, headless);

                    easyNetQService.PushData(transactionInsertModel);

                    RedisRemoveAll(subscriber.SubscriberId, data.SessionKey);
                }
                else
                {
                    _logger.Info($"{data.SubscriberName} does not exists.");
                }
            }
        }

        [HttpPost("removeRedisByTag")]
        public void RemoveRedisByTag([FromForm] string tag)
        {
            redisService.RedisDatabase.RemoveByTagAsync(tag);
        }

        [HttpPost("removeRedis")]
        public void RemoveRedis([FromForm] int SubscriberId, [FromForm] string SessionKey)
        {
            RedisRemoveAll(SubscriberId, SessionKey);
        }

        [HttpPost("removeRedisBySubcriberId")]
        public void SearchKeys([FromForm] string SubcriberId)
        {
            string pattern = "*:" + SubcriberId + ":*";
            var keys = redisService.RedisDatabase.SearchKeysAsync(pattern).Result;
            _ = redisService.RedisDatabase.RemoveAllAsync(keys.ToArray()).Result;
        }


        [HttpPost("cle")]
        public void ClientTransException([FromBody] ClientTransactionExceptionModel logsData)
        {
            logsData.UserAgent = Request.Headers["User-Agent"];

            clientTransactionService.InsertClientTransactionException(logsData.Function, logsData.Exception, logsData.UserAgent);
        }

        [HttpGet("hc")]
        public void HealthCheck()
        {
            // Method intentionally left empty.            
        }

        private void RedisAddAsync<T>(string redisKey, T value)
        {
            redisService.RedisDatabase.AddAsync(redisKey, value, DateTimeOffset.UtcNow.AddMinutes(redisExpired), when: StackExchange.Redis.When.NotExists);
        }

        private void RedisReplaceAsync<T>(string redisKey, T value)
        {
            redisService.RedisDatabase.ReplaceAsync(redisKey, value, DateTimeOffset.UtcNow.AddMinutes(redisExpired));
        }

        private void RedisRemoveAll(int subscriberId, string sessionKey)
        {
            var arrRedisKey = new List<string>();

            foreach (var key in Enum.GetNames(typeof(RedisKey)))
            {
                var redisKey = redisService.GetRedisKey(key, subscriberId, sessionKey);

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
                SecChUa = Request.Headers["sec-ch-ua"].Count > 0 ? Request.Headers["sec-ch-ua"].ToString() : "",
                SecChUaPlatform = Request.Headers["sec-ch-ua-platform"].Count > 0 ? Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "") : "",
                SecChUaMobile = Request.Headers["sec-ch-ua-mobile"].Count > 0 ? Request.Headers["sec-ch-ua-mobile"].ToString() : ""
            };

            var redisKey = redisService.GetRedisKey(RedisKey.Header.ToString(), SubscriberId, SessionKey);

            if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
            {
                RedisAddAsync(redisKey, headerModel);
            }
            else
            {
                RedisReplaceAsync(redisKey, headerModel);
            }
        }

        private void RedisRemoveAll(int subscriberId, string sessionKey)
        {
            var arrRedisKey = new List<string>();

            foreach (var key in Enum.GetNames(typeof(RedisKey)))
            {
                var redisKey = redisService.GetRedisKey(key, subscriberId, sessionKey);

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
                secChUa = Request.Headers["sec-ch-ua"].Count > 0 ? Request.Headers["sec-ch-ua"].ToString() : "",
                secChUaPlatform = Request.Headers["sec-ch-ua-platform"].Count > 0 ? Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "") : "",
                secChUaMobile = Request.Headers["sec-ch-ua-mobile"].Count > 0 ? Request.Headers["sec-ch-ua-mobile"].ToString() : ""
            };

            var redisKey = redisService.GetRedisKey(RedisKey.Header.ToString(), SubscriberId, SessionKey);

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
