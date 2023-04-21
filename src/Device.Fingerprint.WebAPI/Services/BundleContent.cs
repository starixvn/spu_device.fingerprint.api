using Device.FingerPrint.Core;
using Sentry.Protocol;

namespace Device.Fingerprint.WebAPI.Services
{
    public class BundleContent
    {
        private readonly string scriptContent;
        private readonly IAppSettings appSettings;
        private readonly int subscriberId;
        private readonly string sessionKey;
        private readonly string scheme;

        public BundleContent(int subscriberId, string scriptContent, IAppSettings appSettings, string sessionKey, string scheme)
        {
            this.scriptContent = scriptContent;
            this.appSettings = appSettings;
            this.subscriberId = subscriberId;
            this.sessionKey = sessionKey;
            this.scheme = scheme;
        }

        public string Content
        {
            get
            {
                var apiUrl = scheme + ":" + appSettings.ApiUrl;
                var portalApiUrl = scheme + ":" + appSettings.PortalApiUrl;

                var script = this.scriptContent                    
                    .Replace("__subscriberId__", subscriberId.ToString())
                    .Replace("__sessionKey__", sessionKey)
                    .Replace("__apiUrl__", $"{apiUrl}")
                    .Replace("__portalAPIUrl__", $"{portalApiUrl}")
                    .Replace("__version__", "1.3");

                return script;
            }
        }
    }
}
