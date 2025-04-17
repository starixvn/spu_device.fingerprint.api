namespace Device.Identification.ActivatorScript
{
    using Device.Identification.Services;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Web;

    public class ScriptLoader(IAppSettings appSettings, JwtToken jwtToken, NumberGenerator numberGenerator, BundleConfig bundleConfig)
    {
        private readonly IAppSettings _appSettings = appSettings;
        private readonly JwtToken _jwtToken = jwtToken;
        private readonly NumberGenerator _numberGenerator = numberGenerator;
        private readonly BundleConfig _bundleConfig = bundleConfig;

        private readonly string _base64Key = "wFKfVDGZqRbyYCBi6IveMaQAh1ntWcP8ujT/U7r2xXs+J0L=4d3zEHSo5glN9kOpm";

        public string LoadConfigScript(string cachedDeviceId, string defaultDeviceId, string accept, string headers, string domainName, string fdiKey)
        {
            var configScript = _bundleConfig.GetBundle("creditConfig");
            var userHostAddress = Utilities.GetUserHostAddress();
            var ip2Long = IP2Long(userHostAddress);

            configScript = configScript
                    .Replace("__host__", _appSettings.ScriptHost.Replace("__domainName__", domainName))
                    .Replace("__cdn__", _appSettings.ScriptHostCDN.Replace("__domainName__", domainName))
                    .Replace("127.0.0.1", HttpUtility.JavaScriptStringEncode(userHostAddress))
                    .Replace("2130706433", ip2Long.ToString())
                    .Replace("__cookieStorage__", HttpUtility.JavaScriptStringEncode(_appSettings.CookieStorage))
                    .Replace("__version__", HttpUtility.JavaScriptStringEncode(_appSettings.ScriptVersion))
                    .Replace("__encryptpassword__", HttpUtility.JavaScriptStringEncode(_appSettings.EncryptPassword))
                    .Replace("__separator__", HttpUtility.JavaScriptStringEncode(_appSettings.Separator))
                    .Replace("__na__", HttpUtility.JavaScriptStringEncode(Configurations.EmptyValue))
                    .Replace("__storages__", HttpUtility.JavaScriptStringEncode(_appSettings.Storages))
                    .Replace("__cachedDeviceId__", cachedDeviceId)
                    .Replace("__defaultDeviceId__", string.IsNullOrEmpty(cachedDeviceId) ? defaultDeviceId : cachedDeviceId)
                    .Replace("__accept__", accept)
                    .Replace("__headers__", headers)
                    .Replace("__capturedDate__", DateTime.Now.Ticks.ToString())
                    .Replace("__sessionKey__", GetSessionKey(fdiKey, cachedDeviceId, defaultDeviceId))
                    .Replace("__dbIndex__", GetIndexDB().ToString())
                    .Replace("__apiUrl__", GetFPAPIUrl(domainName))
                    .Replace("__portalAPIUrl__", GetPortalAPIUrl(domainName))
                    .Replace("__key__", _base64Key)
                    .Replace("__jsChallengeVersion__", _appSettings.JsChallengeVersion)
                    .Replace("__jsChallengeAPIUrl__", GetJsChallengeAPIUrl());

            return configScript;
        }

        private string GetFPAPIUrl(string domainName)
        {
            const string searchPattern = "sc.";
            const string replacePattern = "fpc.";
            const string placeholder = "__fpcDomainName__";

            var updatedDomainName = domainName.Replace(searchPattern, replacePattern);
            return _appSettings.FPAPIUrl.Replace(placeholder, updatedDomainName);
        }

        private string GetPortalAPIUrl(string domainName)
        {
            const string searchPattern = "sc.";
            const string replacePattern = "fpp.";
            const string placeholder = "__fppDomainName__";

            var updatedDomainName = domainName.Replace(searchPattern, replacePattern);
            return _appSettings.FPPortalAPIUrl.Replace(placeholder, updatedDomainName);
        }

        public static string GetSessionKey(string fdiKey, string cachedDeviceId, string defaultDeviceId)
        {
            return !string.IsNullOrEmpty(fdiKey) ? fdiKey
                 : !string.IsNullOrEmpty(cachedDeviceId) ? cachedDeviceId
                 : defaultDeviceId;
        }

        public int GetIndexDB()
        {
            return _numberGenerator.GetNextNumber();
        }

        public string GetJsChallengeAPIUrl()
        {
            if (!_appSettings.DisableJsChallenge)
            {
                return _appSettings.JsChallengeAPIUrl;
            }

            return string.Empty;
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
