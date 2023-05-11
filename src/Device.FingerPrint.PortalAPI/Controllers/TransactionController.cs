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
using Newtonsoft.Json;
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
        private readonly ICrypto crypto;
        private readonly ILogger logger;
        private readonly IAppSettings appSettings;
        private readonly IBrowserInfoService browserInfoService;

        public TransactionController(
            IAppSettings appSettings,
            IRedisService redisService,
            ITransactionService transactionService,
            IEasyNetQService easyNetQService,
            ISubscriberServices subscriberServices,
            IClientTransactionService clientTransactionService,
            IBrowserInfoService browserInfoService,
            ILogger logger,
            ICrypto crypto)
        {
            this.redisService = redisService;
            this.transactionService = transactionService;
            this.easyNetQService = easyNetQService;
            this.subscriberServices = subscriberServices;
            this.clientTransactionService = clientTransactionService;
            this.browserInfoService = browserInfoService;
            this.appSettings = appSettings;
            this.crypto = crypto;
            this.logger = logger;
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

                    var redisKey = redisService.GetRedisKey(RedisKey.Cookie, deserilizeData.SubscriberId, deserilizeData.SessionKey);

                    RedisUpdate(redisKey, deserilizeData.Cookie);

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

                    var redisKey = redisService.GetRedisKey(RedisKey.Headless, deserilizeData.SubscriberId, deserilizeData.SessionKey);

                    RedisUpdate(redisKey, deserilizeData.HeadlessBrowser);

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

                    var redisKey = redisService.GetRedisKey(RedisKey.BrowserInfo, deserilizeData.SubscriberId, deserilizeData.SessionKey);

                    RedisUpdate(redisKey, deserilizeData.BrowserDetect);
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

                    var redisKey = redisService.GetRedisKey(RedisKey.FingerPrint, deserilizeData.SubscriberId, deserilizeData.SessionKey);

                    RedisUpdate(redisKey, deserilizeData.fingerprint);
                }
            }
        }

        [HttpPost("transaction")]
        public void Transaction([FromBody] ClientTransactionModel data)
        {
            return;

            if (data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SessionKey) && !string.IsNullOrWhiteSpace(data.SubscriberName))
            {
                var subscriber = subscriberServices.GetSubscriberByName(data.SubscriberName);

                if (subscriber != null)
                {
                    var urlAddress = new Uri(data.Url);
                    var userAgentCode = crypto.GetMD5Hash(data.UserAgent);

                    var fingerPrint = redisService.GetFingerPrint(subscriber.SubscriberId, data.SessionKey);
                    var header = redisService.GetHeader(subscriber.SubscriberId, data.SessionKey);
                    var browserInfo = redisService.GetBrowserInfo(subscriber.SubscriberId, data.SessionKey);
                    var cookie = redisService.GetCookie(subscriber.SubscriberId, data.SessionKey);
                    var headless = redisService.GetHeadless(subscriber.SubscriberId, data.SessionKey);

                    var userAgentInfo = transactionService.ParseUserAgentInfo(data.UserAgent);                                        

                    var browserInfoModel = browserInfoService.GetBrowserInfo(header.Result, browserInfo.Result);

                    browserInfoModel.HeadlessScore = headless.Result.Score;
                    browserInfoModel.HeadlessDetail = headless.Result.TestsDetail;

                    var browserInfoAttributeModel = browserInfoService.GetBrowserInfoAttribute(header.Result, browserInfo.Result);
                    

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
                        SubAccId = data.SubAccId,
                        UserAgentInfo = userAgentInfo,
                        CookieCode = cookie.Result,
                        BrowserInfo = browserInfoModel,
                        BrowserInfoAttribute = browserInfoAttributeModel,
                        FingerprintAttribute = transactionService.TransformFingerprintAttributes(fingerPrint.Result, header.Result, browserInfo.Result, userAgentInfo)
                    };

                    easyNetQService.PushData(transaction);

                    RedisRemoveAll(subscriber.SubscriberId, data.SessionKey);

                }
                else
                {
                    logger.Info($"{data.SubscriberName} does not exists.");
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

        [HttpGet("etg/{subscriberId}/{sessionKey}/{cookie}")]
        public void Etag(int subscriberId, string sessionKey, string cookie)
        {
            if (HttpContext.Request.Headers.Keys.Contains("If-None-Match") && HttpContext.Request.Headers["If-None-Match"].ToString() == cookie)
            {
                HttpContext.Response.StatusCode = 304;
            }

            HttpContext.Response.Headers.Add("ETag", new[] { cookie });

            var redisKey = redisService.GetRedisKey(RedisKey.Cookie, subscriberId, sessionKey);

            RedisUpdate(redisKey, cookie);
        }

        private void RedisUpdate<T>(string redisKey, T value)
        {
            if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
            {
                RedisAddAsync(redisKey, value);
            }
            else
            {
                RedisReplaceAsync(redisKey, value);
            }
        }

        private void RedisAddAsync<T>(string redisKey, T value)
        {
            redisService.RedisDatabase.AddAsync(redisKey, value, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired), when: StackExchange.Redis.When.NotExists);
        }

        private void RedisReplaceAsync<T>(string redisKey, T value)
        {
            redisService.RedisDatabase.ReplaceAsync(redisKey, value, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
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
            var url = HttpContext.Request.Headers["Origin"].ToString();
            var headerModel = new HeaderModel
            {
                Accept = HttpContext.Request.Headers["Accept"],
                Encoding = HttpContext.Request.Headers["Accept-Encoding"],
                Headers = String.Join(" ", HttpContext.Request.Headers.Keys),
                SecChUa = Request.Headers["sec-ch-ua"].Count > 0 ? Request.Headers["sec-ch-ua"].ToString() : "",
                SecChUaPlatform = Request.Headers["sec-ch-ua-platform"].Count > 0 ? Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "") : "",
                SecChUaMobile = Request.Headers["sec-ch-ua-mobile"].Count > 0 ? Request.Headers["sec-ch-ua-mobile"].ToString() : "",
                Url = url
            };

            var redisKey = redisService.GetRedisKey(RedisKey.Header, SubscriberId, SessionKey);

            if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
            {
                RedisAddAsync(redisKey, headerModel);
            }
            else
            {
                RedisReplaceAsync(redisKey, headerModel);
            }
        }

    }
}
