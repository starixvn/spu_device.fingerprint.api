using System;
using System.Text;

namespace Device.Fingerprint.WebAPI.Services
{
    public class BundleContent
    {
        private readonly string scriptContent;
        private readonly IAppSettings appSettings;
        private readonly int subscriberId;
        private readonly string sessionKey;

        public BundleContent(int subscriberId, string scriptContent, IAppSettings appSettings, string sessionKey)
        {
            this.scriptContent = scriptContent;
            this.appSettings = appSettings;
            this.subscriberId = subscriberId;
            this.sessionKey = sessionKey;
        }

        public string Content
        {
            get
            {
                var script = this.scriptContent
                    .Replace("__subscriberId__", subscriberId.ToString())
                    .Replace("__sessionKey__", sessionKey)
                    .Replace("__apiUrl__", $"{appSettings.ApiUrl}/api/fp");

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
