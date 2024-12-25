using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Extensions;

namespace Device.Identification
{
    public class Utilities
    {
        private static readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        public static string GetUserHostAddress()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;

            var ip = request.Headers["X-Forwarded-For"].FirstOrDefault();

            if (string.IsNullOrEmpty(ip))
            {
                ip = request.HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            if (ip == "::1")
            {
                return "127.0.0.1";
            }

            return ip?.Split(',')[0].Trim() ?? string.Empty;
        }

        public static string GetLogMessage(Exception ex)
        {
            return new LogMessage(ex.Message)
                   .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                   .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                   .ToString();
        }
    }
}
