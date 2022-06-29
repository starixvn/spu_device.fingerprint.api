using Device.Fingerprint.WebAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Fingerprint.WebAPI.Ultilities
{
    public static class Validation
    {
        public static bool SessionKey(string sessionKey)
        {
            if (!string.IsNullOrWhiteSpace(sessionKey) && sessionKey.Length == 62)
            {
                var getDate = sessionKey.Substring(8, 8);
                var getGUID = sessionKey.Substring(16, 32);
                var getTime = sessionKey.Substring(48, 6);

                bool isValid = Guid.TryParse(getGUID, out var result);

                if (isValid)
                {
                    var userDate = DateTime.ParseExact(getDate + getTime, "yyyyMMddhhmmss", null);

                    if (DateTime.UtcNow.Subtract(userDate).TotalMinutes < 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool DuplicateSessionKey(IRedisService redisService, string sessionKey, int subscriberId)
        {
            var redisKey = string.Format("{0}:{1}", subscriberId, sessionKey);

            if (!redisService.RedisDatabase.ExistsAsync(redisKey).Result)
            {
                redisService.RedisDatabase.AddAsync<string>(redisKey, sessionKey, DateTimeOffset.UtcNow.AddMinutes(1), StackExchange.Redis.When.NotExists);

                return false;
            }

            return true;
        }
    }
}
