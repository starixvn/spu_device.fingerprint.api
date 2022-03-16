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
                    .Replace("__apiUrl__", $"{appSettings.ApiUrl}/api");

                return script;
            }
        }
    }
}
