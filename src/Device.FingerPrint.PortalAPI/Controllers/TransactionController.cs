namespace Device.FingerPrint.PortalAPI.Controllers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Features.ClientTransaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.PortalAPI.Models;
    using Device.FingerPrint.Redis;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.Linq;

    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IRedisService redisService;
        private readonly IClientTransactionService clientTransactionService;
        private readonly IAppSettings appSettings;

        public TransactionController(
            IAppSettings appSettings,
            IRedisService redisService,
            IClientTransactionService clientTransactionService)
        {
            this.redisService = redisService;
            this.clientTransactionService = clientTransactionService;
            this.appSettings = appSettings;
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ce")]
        public void Cookie([FromBody] CookieParameter data)
        {
            if (data != null)
            {
                AddHeaderToRedis(data.SessionKey);
                redisService.HashSetAndExpiryAsync(data.SessionKey, RedisString.GetRedisKeyName(RedisKey.Cookie), data.Cookie, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("fp")]
        public void FingerPrint([FromBody] FingerprintParameter data)
        {
            if (data != null)
            {
                redisService.HashSetAndExpiryAsync(data.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint), data.fingerprint, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
            }
        }

        [HttpPost("transaction")]
        public void Transaction([FromBody] ClientTransactionModel data)
        {
            
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

            redisService.HashSetAndExpiryAsync(sessionKey, RedisString.GetRedisKeyName(RedisKey.Cookie), cookie, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
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
    }
}
