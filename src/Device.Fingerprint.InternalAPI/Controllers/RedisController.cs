using Device.FingerPrint.Redis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Device.Fingerprint.InternalAPI.Controllers
{
    [Route("api/redis")]
    [ApiController]
    public class RedisController(IRedisMultiplexDbService redisMultiplexService) : ControllerBase
    {
        private readonly IRedisMultiplexDbService redisMultiplexService = redisMultiplexService;

        [HttpGet("get-value-redis-string")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetRedisString(int dbIndex, string key)
        {
            var redisDB = redisMultiplexService.GetDatabase(dbIndex);
            var result = redisDB.StringGet(key);

            if (result.HasValue)
            {
                // Get TTL (Time To Live) for the key
                var ttlSeconds = await redisMultiplexService.GetTimeSpan(dbIndex, key);
                
                string expiryInfo;
                if (ttlSeconds > 0)
                {
                    var timeSpan = TimeSpan.FromSeconds(ttlSeconds);
                    expiryInfo = $"Expires in: {timeSpan.TotalSeconds:F0} seconds ({timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s)";
                }
                else if (ttlSeconds == -1)
                {
                    expiryInfo = "No expiration set (persistent key)";
                }
                else
                {
                    expiryInfo = "Key has expired or does not exist";
                }

                return Ok($"Key '{key}' found in database {dbIndex} with value: {result}. {expiryInfo}");
            }
            else
            {
                return NotFound($"Key '{key}' not found in database {dbIndex}.");
            }
        }

        [HttpPost("add-update-redis-string")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> AddUpdateRedisString(int dbIndex, string key, string value, long? expireSeconds = null)
        {
            var redisDB = redisMultiplexService.GetDatabase(dbIndex);
            bool result;

            if (expireSeconds.HasValue && expireSeconds.Value > 0)
            {
                // Set string with expiration
                var expiry = TimeSpan.FromSeconds(expireSeconds.Value);
                result = await redisDB.StringSetAsync(key, value, expiry);
                
                if (result)
                {
                    return Ok($"Key '{key}' added with value '{value}' in database {dbIndex} with expiration of {expireSeconds.Value} seconds.");
                }
                else
                {
                    return BadRequest($"Failed to add key '{key}' with expiration in database {dbIndex}.");
                }
            }
            else
            {
                // Set string without expiration (persistent)
                result = redisDB.StringSet(key, value);
                
                if (result)
                {
                    return Ok($"Key '{key}' added with value '{value}' in database {dbIndex} (no expiration).");
                }
                else
                {
                    return BadRequest($"Failed to add key '{key}' in database {dbIndex}.");
                }
            }
        }

        [HttpPost("delete-redis-string")]
        [AllowAnonymous]
        public ActionResult<string> DeleteRedisString(int dbIndex, string key)
        {
            var redisDB = redisMultiplexService.GetDatabase(dbIndex);
            var result = redisDB.KeyDelete(key);

            if (result)
            {
                return Ok($"Key '{key}' deleted successfully from database {dbIndex}.");
            }
            else
            {
                return BadRequest($"Failed to delete key '{key}' from database {dbIndex}. Key may not exist.");
            }
        }
    }
}
