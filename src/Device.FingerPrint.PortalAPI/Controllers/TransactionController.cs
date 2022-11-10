using Device.FingerPrint.Model;
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
        public void Cookie([FromBody] ClientTransactionCookieModel data)
        {
            if(!redisService.RedisDatabase.ExistsAsync(data.SessionKey).Result)
            {
                redisService.RedisDatabase.AddAsync(data.SessionKey, data, DateTimeOffset.UtcNow.AddMinutes(1), when: StackExchange.Redis.When.NotExists);
            }
        }

        [HttpGet("getcookie")]
        public ClientTransactionCookieModel GetCookie(string sessionKey)
        {
            return redisService.RedisDatabase.GetAsync<ClientTransactionCookieModel>(sessionKey).Result;
        }
    }
}
