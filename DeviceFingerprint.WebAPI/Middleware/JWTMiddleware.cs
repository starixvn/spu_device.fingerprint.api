using DeviceFingerprint.WebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Middleware
{
    public class JWTMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IAppSettings appSettings;

        public JWTMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            this.appSettings = new AppSettings(configuration);
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null && !ValidateToken(context, token))
            {
                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await next(context);
        }

        private bool ValidateToken(HttpContext context, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var JWTService = new JWT(appSettings);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(JWTService.keyJWT),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                context.Items["key"] = "Accepted";
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
