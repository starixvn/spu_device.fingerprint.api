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
            if(!redisService.CookieDatabase.ExistsAsync(data.RedisKey).Result)
            {
                redisService.CookieDatabase.AddAsync(data.RedisKey, data.ClientTransCookie, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
            }
        }

        [HttpPost("headless")]
        public void Headless([FromBody] HeadlessParameter data)
        {
            if (!redisService.HeadlessDatabase.ExistsAsync(data.RedisKey).Result)
            {
                redisService.HeadlessDatabase.AddAsync(data.RedisKey, data.HeadlessBrowser, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
            }
        }

        [HttpGet("getcookie")]
        public ClientTransactionCookieModel GetCookie(string sessionKey)
        {
            return redisService.RedisDatabase.GetAsync<ClientTransactionCookieModel>(sessionKey).Result;
        }
    }
}
