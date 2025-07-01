using Device.FingerPrint.Redis;
using Device.Fingerprint.InternalAPI.Model;
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
        public async Task<ActionResult<string>> AddUpdateRedisString([FromBody] RedisStringRequest request)
        {
            var redisDB = redisMultiplexService.GetDatabase(request.DbIndex);
            bool result;

            if (request.ExpireSeconds.HasValue && request.ExpireSeconds.Value > 0)
            {
                // Set string with expiration
                var expiry = TimeSpan.FromSeconds(request.ExpireSeconds.Value);
                result = await redisDB.StringSetAsync(request.Key, request.Value, expiry);
                
                if (result)
                {
                    return Ok($"Key '{request.Key}' added with value '{request.Value}' in database {request.DbIndex} with expiration of {request.ExpireSeconds.Value} seconds.");
                }
                else
                {
                    return BadRequest($"Failed to add key '{request.Key}' with expiration in database {request.DbIndex}.");
                }
            }
            else
            {
                // Set string without expiration (persistent)
                result = redisDB.StringSet(request.Key, request.Value);
                
                if (result)
                {
                    return Ok($"Key '{request.Key}' added with value '{request.Value}' in database {request.DbIndex} (no expiration).");
                }
                else
                {
                    return BadRequest($"Failed to add key '{request.Key}' in database {request.DbIndex}.");
                }
            }
        }

        [HttpPost("delete-redis-string")]
        [AllowAnonymous]
        public ActionResult<string> DeleteRedisString([FromBody] RedisDeleteRequest request)
        {
            var redisDB = redisMultiplexService.GetDatabase(request.DbIndex);
            var result = redisDB.KeyDelete(request.Key);

            if (result)
            {
                return Ok($"Key '{request.Key}' deleted successfully from database {request.DbIndex}.");
            }
            else
            {
                return BadRequest($"Failed to delete key '{request.Key}' from database {request.DbIndex}. Key may not exist.");
            }
        }
    }
}
