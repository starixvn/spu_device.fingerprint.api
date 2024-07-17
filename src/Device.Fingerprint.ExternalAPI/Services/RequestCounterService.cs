using Device.FingerPrint.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Fingerprint.ExternalAPI.Services
{
    public class RequestCounterService
    {
        private readonly IRedisService redisService;
        private const string HashKey = "request_counters";        

        public RequestCounterService(IRedisService redisService)
        {
            this.redisService = redisService;
        }        

        public async Task IncrementRequestCountAsync(string functionName)
        {             
            await this.redisService.HashIncrementAsync(HashKey + "_" + functionName, DateTime.UtcNow.ToString("yyyy-MM-dd"), 1);
        }
    }
}
