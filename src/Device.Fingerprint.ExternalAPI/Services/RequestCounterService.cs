using Device.FingerPrint.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Fingerprint.ExternalAPI.Services
{
    public class RequestCounterService
    {
        private readonly IRedisService redisService;        

        public RequestCounterService(IRedisService redisService)
        {
            this.redisService = redisService;
        }        

        public async Task IncrementRequestCountAsync(string functionName)
        {             
            await this.redisService.HashIncrementAsync(functionName, DateTime.UtcNow.ToString("yyyy-MM-dd"), 1);
        }
    }
}
