using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.PortalAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IRedisService redisService;
        
        public TransactionController(IRedisService redisService)
        {
            this.redisService = redisService;
        }

        [HttpPost("cookie")]
        public void Cookie([FromBody] CookieParameter data)
        {
            if(data != null)
            {
                var redisKey = $"cookie:{data.SubscriberId}:{data.RedisKey}";

                //if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                //{

                //    redisService.RedisDatabase.AddAsync(redisKey, data.Cookie, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
                //}
            }           
        }

        [HttpPost("headless")]
        public void Headless([FromBody] HeadlessParameter data)
        {
            if (data != null)
            {
                var redisKey = $"headless:{data.SubscriberId}:{data.RedisKey}";

                //if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                //{
                //    redisService.RedisDatabase.AddAsync(redisKey, data.HeadlessBrowser, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
                //}
            }
                
        }

        [HttpPost("browserinfo")]
        public void BrowserInfo([FromBody] BrowserDetectParameter data)
        {
            if(data != null)
            {
                var redisKey = $"browserinfo:{data.SubscriberId}:{data.RedisKey}";

                //if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                //{
                //    redisService.RedisDatabase.AddAsync(redisKey, data.BrowserDetect, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
                //}
            }
            
        }

        [HttpPost("fingerprint")]
        public void FingerPrint([FromBody] FingerprintParameter data)
        {
            if(data != null)
            {
                var redisKey = $"fingerprint:{data.SubscriberId}:{data.RedisKey}";

                //if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
                //{
                //    redisService.RedisDatabase.AddAsync(redisKey, data.fingerprint, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
                //}
            }
            
        }

        [HttpGet("getcookie")]
        public ClientTransactionCookieModel GetCookie(string sessionKey)
        {
            return redisService.RedisDatabase.GetAsync<ClientTransactionCookieModel>(sessionKey).Result;
        }
    }
}
