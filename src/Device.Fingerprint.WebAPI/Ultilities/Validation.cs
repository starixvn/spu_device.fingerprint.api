using Device.Fingerprint.WebAPI.Services;
using System;
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

                    if (DateTime.UtcNow.Subtract(userDate).TotalMinutes < 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static async Task<bool> CheckExistingSessionKey(IRedisService redisService, string sessionKey, int subscriberId)
        {
            var redisKey = string.Format("{0}:{1}", subscriberId, sessionKey);

            return await redisService.RedisDatabase.ExistsAsync(redisKey);
        }

        public static void AddSessionKey(IRedisService redisService, string sessionKey, int subscriberId)
        {
            var redisKey = string.Format("{0}:{1}", subscriberId, sessionKey);

            redisService.RedisDatabase.AddAsync(redisKey, sessionKey, DateTimeOffset.UtcNow.AddMinutes(1), StackExchange.Redis.When.NotExists);
        }


    }
}
