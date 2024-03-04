using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Linq;
using Device.FingerPrint.Core;
using Device.Fingerprint.ExternalAPI.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Device.Fingerprint.ExternalAPI.Middleware
{
    public class JWTValidationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IAppSettings appSettings;

        public JWTValidationMiddleware(RequestDelegate next, IAppSettings appSettings)
        {
            this.next = next;
            this.appSettings = appSettings;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Value.Contains("generatetoken/audience"))
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(' ').Last();

                if (string.IsNullOrWhiteSpace(token) || !ValidateToken(context, token))
                {
                    context.Response.Clear();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                    return;
                }
            }

            await next(context);
        }

        private bool ValidateToken(HttpContext context, string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                

                var jwtSetting = appSettings.Configuration.GetSection("AppConfig:JWT:SubscriberConfig").Get<IEnumerable<SubscrbierTokenModel>>();
                var subscriber = context.Request.Path.Value.Split('/')[3];
                foreach (var item in jwtSetting)
                {
                    if (item.Token.ToLower().Equals(token.ToLower()) && item.Subscribers.Contains(subscriber, StringComparison.CurrentCultureIgnoreCase)) return true;
                }
            }

            return false;

        }
    }
}
