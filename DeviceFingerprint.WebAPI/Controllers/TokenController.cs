using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using DeviceFingerprint.WebAPI.Services;
using Microsoft.Extensions.Configuration;
using DeviceFingerprint.WebAPI.Models;

namespace DeviceFingerprint.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment environment;
        private readonly IJWTAuthenticationManager JWTAuthenticationManager;

        public TokenController(IWebHostEnvironment Environment, IJWTAuthenticationManager JWTAuthenticationManager, IConfiguration configuration)
        {
            this.environment = Environment;
            this.JWTAuthenticationManager = JWTAuthenticationManager;
            this.Configuration = configuration;
        }

        [HttpGet]
        public ActionResult<TokenResultModel> Get(int subscriberId)
        {
            var tokenJWT = JWTAuthenticationManager.GenerateToken(subscriberId);
            return new TokenResultModel
            {
                Token = tokenJWT
            };
        }
    }
}
