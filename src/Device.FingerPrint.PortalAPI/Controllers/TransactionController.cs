namespace Device.FingerPrint.PortalAPI.Controllers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Encoders;
    using Device.FingerPrint.Features.ClientTransaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.PortalAPI.Models;
    using Device.FingerPrint.Redis;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IRedisService redisService;
        private readonly IRedisMultiplexService redisMultiplexService;
        private readonly IClientTransactionService clientTransactionService;
        private readonly IAppSettings appSettings;
        private const string base64Key = "wFKfVDGZqRbyYCBi6IveMaQAh1ntWcP8ujT/U7r2xXs+J0L=4d3zEHSo5glN9kOpm";
        private const string originService = "PortalAPI";

        public TransactionController(
            IAppSettings appSettings,
            IRedisService redisService,
            IRedisMultiplexService redisMultiplexService,
            IClientTransactionService clientTransactionService)
        {
            this.redisService = redisService;
            this.redisMultiplexService = redisMultiplexService;
            this.clientTransactionService = clientTransactionService;
            this.appSettings = appSettings;
        }

        [HttpPost("h/d/{version}")]
        public void Header([FromBody] string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var decodeData = Base64.Decode(data, base64Key);
                var header = JsonSerializer.Deserialize<BaseParameter>(decodeData);
                AddHeaderToRedis(header.SessionKey);
            }
        }

        [HttpPost("c/e/{version}")]
        public async Task Cookie([FromBody] string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var decodeData = Base64.Decode(data, base64Key);
                var cookie = JsonSerializer.Deserialize<CookieParameter>(decodeData);

                if (cookie != null)
                {
                    var cookieValue = cookie.Cookie;
                    var cookieRedis = await redisService.HashGetAsync<string>(cookie.SessionKey, RedisString.GetRedisKeyName(RedisKey.Cookie));
                    var isForceUpdate = cookieRedis != null && cookieRedis != cookieValue;

                    await redisService.HashSetAndExpiryAsync(cookie.SessionKey, RedisString.GetRedisKeyName(RedisKey.Cookie), cookie.Cookie, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired), isForceUpdate);
                }

                AddHeaderToRedis(cookie.SessionKey);
            }
        }

        [HttpPost("f/p/{version}")]
        public async Task FingerPrint([FromBody] string data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                var decodeData = Base64.Decode(data, base64Key);
                var fingerprint = JsonSerializer.Deserialize<FingerprintParameter>(decodeData);

                if (fingerprint != null)
                {
                    var fingerprintCode = fingerprint.Fingerprint?.Id;

                    var fingerPrintRedis = await redisService.HashGetAsync<FingerprintModel>(fingerprint.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint));
                    var isForceUpdate = fingerPrintRedis != null && fingerPrintRedis.Id != fingerprintCode;

                    await redisService.HashSetAndExpiryAsync(fingerprint.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint), fingerprint.Fingerprint, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired), isForceUpdate);
                }
            }
        }

        [HttpPost("h/d")]
        [HttpPost("hd")]
        public void Header([FromBody] PortalParameter data)
        {
            if (data != null)
            {
                var sessionKey = Utilities.ParseSessionKey(data.SessionKey, appSettings.RedisDatabase);

                if (sessionKey != null && sessionKey.RedisDbIndex > -1)
                {
                    AddHeaderToRedisWithDbIndex(sessionKey.RedisDbIndex, sessionKey.SessionKey);
                }
            }
        }

        [HttpPost("c/e")]
        [HttpPost("ce")]
        public void Cookie([FromBody] PortalParameter data)
        {
            if (data != null)
            {
                var sessionKey = Utilities.ParseSessionKey(data.SessionKey, appSettings.RedisDatabase);

                if (sessionKey != null && sessionKey.RedisDbIndex > -1)
                {
                    redisMultiplexService.HashSetWithDbIndexAsync(originService, sessionKey.RedisDbIndex, sessionKey.SessionKey, RedisString.GetRedisKeyName(RedisKey.Cookie), data.Data, appSettings.RedisExpired);
                }
            }
        }

        [HttpPost("f/p")]
        [HttpPost("fp")]
        public void FingerPrint([FromBody] PortalParameter data)
        {
            if (data != null)
            {
                var sessionKey = Utilities.ParseSessionKey(data.SessionKey, appSettings.RedisDatabase);

                if (sessionKey != null && sessionKey.RedisDbIndex > -1)
                {
                    redisMultiplexService.HashSetWithDbIndexAsync(originService, sessionKey.RedisDbIndex, sessionKey.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrintEncode), data.Data, appSettings.RedisExpired);
                    AddHeaderToRedisWithDbIndex(sessionKey.RedisDbIndex, sessionKey.SessionKey);
                }
            }
        }

        [HttpPost("c/l/e")]
        [HttpPost("cle")]
        public void ClientTransException([FromBody] ClientTransactionExceptionModel logsData)
        {
            logsData.UserAgent = Request.Headers["User-Agent"];
            logsData.RealIP = GetRealIp();
            logsData.FakeIP = GetFakeIp();

            clientTransactionService.InsertClientTransactionException(logsData);
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

        [HttpGet("check-redis-expired")]
        public async Task<double> CheckRedisExpired(int redisDbIndex, string sessionKey)
        {
            var timeToLive = await redisMultiplexService.GetTimeSpan(redisDbIndex, sessionKey);

            return timeToLive;
        }

        [HttpGet("hc")]
        public void HealthCheck()
        {
        }

        [HttpGet("hc-sentry")]
        public string HealthCheckSentry()
        {
            LogService.Info("I'm good.");
            return "I'm good.";
        }

        private void AddHeaderToRedis(string sessionKey)
        {
            var headerModel = GetHeaderValue();
            if (headerModel != null)
            {
                redisService.HashSetAndExpiryAsync(sessionKey, RedisString.GetRedisKeyName(RedisKey.Header), headerModel, DateTimeOffset.UtcNow.AddSeconds(appSettings.RedisExpired));
            }
        }

        private void AddHeaderToRedisWithDbIndex(int redisDatabaseIndex, string sessionKey)
        {
            var headerModel = GetHeaderValue();
            if (headerModel != null)
            {
                redisMultiplexService.HashSetWithDbIndexAsync(originService, redisDatabaseIndex, sessionKey, RedisString.GetRedisKeyName(RedisKey.Header), JsonSerializer.Serialize(headerModel), appSettings.RedisExpired);
            }
        }

        private HeaderModel GetHeaderValue()
        {
            var url = HttpContext.Request.Headers["Origin"].ToString();

            return new HeaderModel
            {
                Accept = HttpContext.Request.Headers["Accept"],
                Encoding = NormalizeAcceptEncoding(HttpContext.Request.Headers["Accept-Encoding"]),
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

        static string NormalizeAcceptEncoding(string acceptEncoding)
        {
            if (acceptEncoding == null) return string.Empty;

            var normalizedFormats = acceptEncoding
                .Split(',')
                .Select(format => format.Trim())
                .Where(format => !string.Equals(format, "br", StringComparison.OrdinalIgnoreCase));

            return string.Join(", ", normalizedFormats);
        }
    }
}
