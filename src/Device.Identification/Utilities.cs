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
    }
}
