namespace Device.FingerPrint.PortalAPI.Controllers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Features.ClientTransaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.PortalAPI.Models;
    using Device.FingerPrint.Redis;
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
        [HttpPost("hd")]
        public void Header([FromBody] BaseParameter data)
        {
            if (data != null)
            {
                AddHeaderToRedis(data.SessionKey);
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("ce")]
        public void Cookie([FromBody] CookieParameter data)
        {
            if (data != null)
            {
                redisService.HashSetAndExpiryAsync(data.SessionKey, RedisString.GetRedisKeyName(RedisKey.Cookie), data.Cookie, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
                AddHeaderToRedis(data.SessionKey);
            }
        }

        [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("fp")]
        public void FingerPrint([FromBody] FingerprintParameter data)
        {
            if (data != null)
            {
                redisService.HashSetAndExpiryAsync(data.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint), data.Fingerprint, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
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

        private void AddHeaderToRedis(string sessionKey)
        {
            var headerModel = GetHeaderValue();
            if (headerModel != null)
            {
                redisService.HashSetAsync(sessionKey, RedisString.GetRedisKeyName(RedisKey.Header), headerModel);
            }
        }

        private HeaderModel GetHeaderValue()
        {
            var url = HttpContext.Request.Headers["Origin"].ToString();

            return new HeaderModel
            {
                Accept = HttpContext.Request.Headers["Accept"],
                Encoding = HttpContext.Request.Headers["Accept-Encoding"],
                Headers = string.Join(" ", HttpContext.Request.Headers.Keys),
                SecChUa = Request.Headers["sec-ch-ua"].Count > 0 ? Request.Headers["sec-ch-ua"].ToString() : "",
                SecChUaPlatform = Request.Headers["sec-ch-ua-platform"].Count > 0 ? Request.Headers["sec-ch-ua-platform"].ToString().Replace("\"", "") : "",
                SecChUaMobile = Request.Headers["sec-ch-ua-mobile"].Count > 0 ? Request.Headers["sec-ch-ua-mobile"].ToString() : "",
                Url = url,
                RealIP = GetRealIp(),
                FakeIP = GetFakeIp()
            };
        }

        private string GetFakeIp()
        {
            var proxyIP = GetXForWarded();

            if (!string.IsNullOrEmpty(proxyIP))
            {
                var ips = proxyIP.Split(new char[] { ',' });

                if (ips[0] != GetRealIp())
                {
                    return ips[0];
                }
            }

            return string.Empty;
        }

        private string GetHeaderIP(string param)
        {
            var headerVal = HttpContext.Request.Headers[param].ToString();
            if (!string.IsNullOrEmpty(headerVal))
            {
                return headerVal;
            }

            return string.Empty;
        }

        private string GetXForWarded()
        {
            var proxyIP = GetHeaderIP("X-Forwarded-For");

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Originating-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-IP");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = GetHeaderIP("X-Remote-Addr");
            }

            if (string.IsNullOrEmpty(proxyIP))
            {
                proxyIP = HttpContext.Request.Host.ToString();
            }

            return proxyIP;
        }

        private string GetRealIp()
        {
            var proxyIP = GetHeaderIP("X-Actual-IP");

            if (string.IsNullOrEmpty(proxyIP))
            {
                var clientIps = GetXForWarded();

                if (!string.IsNullOrEmpty(clientIps))
                {
                    var ips = clientIps.Split(new char[] { ',' });

                    if (ips.Length > 1)
                    {
                        return ips[1];
                    }

                    return ips[0];
                }
                else
                {
                    return HttpContext.Request.Host.ToString();
                }
            }
            else
            {
                return proxyIP.Split(new char[] { ',' })[0];
            }
        }
    }
}
