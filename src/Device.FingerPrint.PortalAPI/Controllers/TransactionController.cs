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
    using Microsoft.Net.Http.Headers;
    using StackExchange.Redis;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IRedisMultiplexDbService redisMultiplexService;
        private readonly IClientTransactionService clientTransactionService;
        private readonly IAppSettings appSettings;
        private readonly IDatabase everTaggingRedisDB;

        public TransactionController(
            IAppSettings appSettings,
            IRedisMultiplexDbService redisMultiplexService,
            IClientTransactionService clientTransactionService)
        {
            this.redisMultiplexService = redisMultiplexService;
            this.clientTransactionService = clientTransactionService;
            this.appSettings = appSettings;
            everTaggingRedisDB = redisMultiplexService.GetDatabase(appSettings.EverTaggingDBIndex);
        }

        [HttpPost("f/p")]
        public void FingerPrint([FromBody] PortalParameter data)
        {
            if (data != null)
            {
                var sessionKey = Utilities.ParseSessionKey(data.SessionKey, appSettings.RedisDatabase);

                if (sessionKey != null && sessionKey.RedisDbIndex > -1)
                {
                    var headerModel = GetHeaderValue();
                    var keyValuePairs = new Dictionary<string, string>
                                        {
                                            { "CreatedTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
                                            { RedisString.GetRedisKeyName(Model.RedisKey.FingerPrintEncode), data.Data },
                                            { RedisString.GetRedisKeyName(Model.RedisKey.Header), JsonSerializer.Serialize(headerModel) }
                                        };

                    redisMultiplexService.HashSetMultipleFieldsWithDbIndexAsync(sessionKey.RedisDbIndex, sessionKey.SessionKey, keyValuePairs, appSettings.RedisExpired);
                }
            }
        }

        [HttpGet("e/t")]
        public IActionResult GetEverTagging()
        {
            var sesionKey = Request.Headers["Res-Ec-Y"].ToString();            
            var etagValue = GetEtagValue();

            if (!string.IsNullOrEmpty(etagValue))
            {
                sesionKey = etagValue;
            }
            else
            {
                var ecValues = Request.Headers["Res-Ec-X"].ToString();

                var everTaggingValues = ecValues.Split(',');

                foreach (var ec in everTaggingValues)
                {
                    if (Utilities.ValidationSessionKey(ec))
                    {       
                        var ecKeyExisting = everTaggingRedisDB.KeyExists($"EverTagging:{ec}");

                        if (ecKeyExisting)
                        {                           
                            if(sesionKey != ec)
                            {
                                everTaggingRedisDB.StringSet($"RealTagging:{sesionKey}", ec);
                                everTaggingRedisDB.KeyExpire($"RealTagging:{sesionKey}", TimeSpan.FromSeconds(3600));

                                sesionKey = ec;                               
                            }

                            break;
                        }                        
                    }
                    else
                    {
                        LogService.Warn($"This is ever tagging that is not validated ec: {ec} + sk: {sesionKey}");
                    }
                }
            }

            UpdateResponseHeaders(sesionKey);

            return Ok(sesionKey);
        }

       
        private void UpdateResponseHeaders(string tagging)
        {
            var ApiEverTaggingCacheMaxAge = appSettings.ApiEverTaggingCacheMaxAge;
            Response.Headers["Cache-Control"] = $"max-age={ApiEverTaggingCacheMaxAge}, private";
            Response.Headers["ETag"] = tagging;
        }

        [HttpPost("c/l/e")]
        public void ClientTransException([FromBody] ClientTransactionExceptionModel logsData)
        {
            logsData.UserAgent = Request.Headers["User-Agent"];
            logsData.RealIP = GetRealIp();
            logsData.FakeIP = GetFakeIp();

            clientTransactionService.InsertClientTransactionException(logsData);
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

        [HttpGet("decrypt/{str}")]
        public string Decrypt(string str)
        {
            return DeviceEncoder.Decrypt(str);
        }

        [HttpGet("encrypt/{str}")]
        public string Encrypt(string str)
        {
            return DeviceEncoder.Encrypt(str);
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

        private HeaderModel GetHeaderValue()
        {
            try
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
            catch (Exception ex)
            {
                LogService.Error(ex.Message);
                return new HeaderModel();
            }
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

        private string GetEtagValue()
        {
            var requestHeaders = Request.Headers;
            if (requestHeaders.TryGetValue(HeaderNames.IfNoneMatch, out var requestETag))
            {
                return requestETag;
            }

            return string.Empty;
        }
    }
}
