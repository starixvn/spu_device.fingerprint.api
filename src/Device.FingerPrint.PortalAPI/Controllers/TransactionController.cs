namespace Device.FingerPrint.PortalAPI.Controllers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Features.ClientTransaction.Services;
    using Device.FingerPrint.Features.Subscriber.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.PortalAPI.Models;
    using Device.FingerPrint.PortalAPI.Services;
    using Device.FingerPrint.PortalAPI.Ultilities;
    using Device.FingerPrint.Redis;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Linq;

    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IRedisService redisService;
        private readonly IEasyNetQService easyNetQService;    
        private readonly ISubscriberServices subscriberServices;
        private readonly IClientTransactionService clientTransactionService;
        private readonly ICrypto crypto;
        private readonly ILogger logger;
        private readonly IAppSettings appSettings;      

        public TransactionController(
            IAppSettings appSettings,
            IRedisService redisService,           
            IEasyNetQService easyNetQService,
            ISubscriberServices subscriberServices,
            IClientTransactionService clientTransactionService,           
            ILogger logger,
            ICrypto crypto)
        {
            this.redisService = redisService;           
            this.easyNetQService = easyNetQService;
            this.subscriberServices = subscriberServices;
            this.clientTransactionService = clientTransactionService;           
            this.appSettings = appSettings;
            this.crypto = crypto;
            this.logger = logger;
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ce")]
        public void Cookie([FromBody] CookieParameter data)
        {
            if (data != null)
            {
                var hashKey = GetHashKey(data.SubscriberId, data.SessionKey);
                AddHeaderToRedis(hashKey);
                redisService.HashSetAndExpiryAsync(hashKey, RedisString.GetRedisKeyName(RedisKey.Cookie), data.Cookie, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("hl")]
        public void Headless([FromBody] HeadlessParameter data)
        {
            if (data != null)
            {
                var hashKey = GetHashKey(data.SubscriberId, data.SessionKey);
                redisService.HashSetAndExpiryAsync(hashKey, RedisString.GetRedisKeyName(RedisKey.Headless), data.HeadlessBrowser, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("bi")]
        public void BrowserInfo([FromBody] BrowserDetectParameter data)
        {
            if (data != null)
            {
                var hashKey = GetHashKey(data.SubscriberId, data.SessionKey);
                redisService.HashSetAndExpiryAsync(hashKey, RedisString.GetRedisKeyName(RedisKey.BrowserInfo), data.BrowserDetect, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("fp")]
        public void FingerPrint([FromBody] FingerprintParameter data)
        {
            if (data != null)
            {
                var hashKey = GetHashKey(data.SubscriberId, data.SessionKey);
                redisService.HashSetAndExpiryAsync(hashKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint), data.fingerprint, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
            }
        }

        [HttpPost("transaction")]
        public void Transaction([FromBody] ClientTransactionModel data)
        {
            if (data != null && !string.IsNullOrWhiteSpace(data.LoginName) && !string.IsNullOrWhiteSpace(data.SubscriberName))
            {
                var subscriber = subscriberServices.GetSubscriberByName(data.SubscriberName);

                if (subscriber != null)
                {
                    var urlAddress = new Uri(data.Url);
                    var userAgentCode = data.UserAgent != null ? crypto.GetMD5Hash(data.UserAgent) : string.Empty;

                    var transaction = new TransactionModel
                    {
                        SessionKey = data.SessionKey,
                        SubscriberId = subscriber.SubscriberId,
                        ClientIP = data.ClientIp,
                        FakeIP = data.FakeIP,
                        LoginName = data.LoginName,
                        UserAgent = data.UserAgent,
                        UserAgentCode = userAgentCode,
                        TransTime = DateTime.Now,
                        Url = urlAddress.GetLeftPart(UriPartial.Path),
                        Action = data.Action,
                        TransKey = data.TransKey,
                        CustId = data.CustId,
                        SubAccId = data.SubAccId,
                        BotD = data.BotD                        
                    };

                    easyNetQService.PushData(transaction);

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

            var redisKey = GetHashKey(subscriberId, sessionKey);

            redisService.HashSetAndExpiryAsync(redisKey, RedisString.GetRedisKeyName(RedisKey.Cookie), cookie, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
        }

        [HttpGet("whoami")]
        public WhoAmIModel WhoAmI()
        {
            var httpConnectionFeature = HttpContext.Features.Get<IHttpConnectionFeature>();
            var localIpAddress = httpConnectionFeature?.LocalIpAddress;

            return new WhoAmIModel
            {
                LocalIP = localIpAddress.ToString(),
                ServerName = Environment.MachineName,
                ServerTime = DateTime.Now
            };
        }

        private void AddHeaderToRedis(string hashKey)
        {
            var url = HttpContext.Request.Headers["Origin"].ToString();
            var headerModel = new HeaderModel
            {
                Accept = HttpContext.Request.Headers["Accept"],
                Encoding = HttpContext.Request.Headers["Accept-Encoding"],
                Headers = string.Join(" ", HttpContext.Request.Headers.Keys),
                SecChUa = Request.Headers["sec-ch-ua"].Count > 0 ? Request.Headers["sec-ch-ua"].ToString() : "",
                SecChUaPlatform = Request.Headers["sec-ch-ua-platform"].Count > 0 ? Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "") : "",
                SecChUaMobile = Request.Headers["sec-ch-ua-mobile"].Count > 0 ? Request.Headers["sec-ch-ua-mobile"].ToString() : "",
                Url = url
            };

            redisService.HashSetAsync(hashKey, RedisString.GetRedisKeyName(RedisKey.Header), headerModel);
        }

        private static string GetHashKey(int subscriberId, string sessionKey)
        {
            return $"{subscriberId}:{sessionKey}";
        }

    }
}
