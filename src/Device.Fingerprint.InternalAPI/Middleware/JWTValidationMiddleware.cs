using Device.Fingerprint.InternalAPI.Model;
using Device.FingerPrint.Core;

namespace Device.Fingerprint.InternalAPI.Middleware
{
    public class JWTValidationMiddleware(RequestDelegate next, IAppSettings appSettings)
    {
        private readonly RequestDelegate next = next;
        private readonly IAppSettings appSettings = appSettings;

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Value.Contains("generatetoken/audience") && !context.Request.Path.Value.Contains("whoami"))
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
                var jwtSetting = appSettings.Configuration.GetSection("AppConfig:JWT:APIConfig").Get<IEnumerable<APIConfigModel>>();
                var path = context.Request.Path.Value;

                var matchedApi = jwtSetting.FirstOrDefault(item => path.StartsWith(item.API));

                if (matchedApi != null && matchedApi.Token == token)
                {
                    return true;
                }
            }

            return false;

        }
    }
}
