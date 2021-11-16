using System;
using System.Text;

namespace DeviceFingerprint.WebAPI.Services
{
    public class BundleContent
    {
        private readonly int subscriberId;
        private readonly string scriptContent;
        private readonly IAppSettings appSettings;

        public BundleContent(int subscriberId, string scriptContent, IAppSettings appSettings)
        {
            this.subscriberId = subscriberId;
            this.scriptContent = scriptContent;
            this.appSettings = appSettings;
        }

        public string Content
        {
            get
            {
                var JWTService = new JWT(appSettings);
                var script = this.scriptContent
                    .Replace("__subscriberId__", subscriberId.ToString())
                    .Replace("__key__", appSettings.EncryptPassword)
                    .Replace("__apiUrl__", $"{appSettings.ApiUrl}/api/fp")
                    .Replace("__token__", JWTService.GenerateJwtToken())
                    .Replace("__fpsKey__", Guid.NewGuid().ToString("N"));

                return script;
            }
        }

        private string GenerateRandomKey(int subscriberId)
        {
            var key = new StringBuilder();
            var rand = new Random();

            key.Append(subscriberId);
            key.Append(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            key.Append(rand.Next(1000, 999999));

            return key.ToString();
        }
    }
}
