namespace Device.Identification.Model
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1609:PropertyDocumentationMustHaveValue", Justification = "Reviewed.")]
    public static class FpsHosting
    {
        private static readonly IConfiguration _configuration;
        private static readonly IHttpContextAccessor _httpContextAccessor;

        static FpsHosting()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            _httpContextAccessor = new HttpContextAccessor();
        }

        public static readonly string CookieStorage = _configuration["AppConfig.CookieStorage"];

        public static readonly string Storages = _configuration["AppConfig.Storages"];

        public static readonly string ScriptHost = _configuration["AppConfig.ScriptHost"];

        public static readonly string ScriptHostCDN = _configuration["AppConfig.ScriptHostCDN"];

        public static readonly string ScriptDomain = GetScriptDomain(ScriptHost);

        public static readonly string ScriptVersion = _configuration["AppConfig.ScriptVersion"];

        public static readonly DateTime ScriptTime = Convert.ToDateTime(_configuration["AppConfig.ScriptTime"]).ToLocalTime();

        public static readonly string EncryptPassword = _configuration["AppConfig.EncryptPassword"];

        public static readonly string Separator = _configuration["AppConfig.Separator"];

        public static readonly int CachingExpiredSeconds = Convert.ToInt32(_configuration["AppConfig.CachingExpiredSeconds"]);

        public static readonly bool IsDisabledDI = Convert.ToBoolean(_configuration["AppConfig.DisableDIScript"]);

        public static readonly string FPAPIUrl = _configuration["AppConfig.FP-API-Url"];

        public static readonly string FPPortalAPIUrl = _configuration["AppConfig.FP-Portal-API-Url"];

        public static readonly string Base64Key = "wFKfVDGZqRbyYCBi6IveMaQAh1ntWcP8ujT/U7r2xXs+J0L=4d3zEHSo5glN9kOpm";

        public static readonly string RedisDbIndex = _configuration["AppConfig.RedisDbIndex"];

        public static readonly bool DisableJsChallenge = Convert.ToBoolean(_configuration["AppConfig.DisableJsChallenge"]);

        public static readonly string JsChallengeVersion = _configuration["AppConfig.JsChallengeVersion"];

        public static readonly string JsChallengeAPIUrl = _configuration["AppConfig.JsChallengeAPIUrl"];

        public static readonly string JwtID = _configuration["AppConfig.JwtID"];

        public static readonly string JwtSecretKey = _configuration["AppConfig.Jwt-SecretKey"];


        public static string GetUserHostAddress
        {
            get
            {
                var currentRequest = _httpContextAccessor.HttpContext?.Request;
                if (currentRequest == null) return string.Empty;

                var ip = currentRequest.Headers["X-Forwarded-For"].FirstOrDefault();

                if (string.IsNullOrEmpty(ip))
                {
                    ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                if (ip == "::1")
                {
                    return "127.0.0.1";
                }

                return ip?.Split(',')[0].Trim() ?? string.Empty;
            }
        }

        private static string GetScriptDomain(string scriptHost)
        {
            if (string.IsNullOrEmpty(scriptHost)) return string.Empty;

            var scriptUri = new Uri(scriptHost);
            return scriptUri.Host;
        }

        public static long IP2Long(string ip)
        {
            string[] ipBytes;
            double num = 0;
            try
            {
                if (!string.IsNullOrEmpty(ip))
                {
                    ipBytes = ip.Split('.');
                    for (int i = ipBytes.Length - 1; i >= 0; i--)
                    {
                        num += (int.Parse(ipBytes[i]) % 256) * Math.Pow(256, 3 - i);
                    }
                }
            }
            catch
            {
            }

            return (long)num;
        }
    }

}
