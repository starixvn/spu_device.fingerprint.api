namespace Device.Identification.ActivatorScript
{
    public class ActivatorScriptFactory(ScriptLoader scriptLoader, BundleConfig bundleConfig)
    {
        private string? _configScript;
        private readonly ScriptLoader _scriptLoader = scriptLoader;
        private readonly BundleConfig _bundleConfig = bundleConfig;

        public ActivatorScriptFactory CreateConfigScript(
            string cachedDeviceId,
            string defaultDeviceId,
            string accept,
            string headers,
            string domainName,
            string fdiKey)
        {
            _configScript = _scriptLoader.LoadConfigScript(cachedDeviceId, defaultDeviceId, accept, headers, domainName, fdiKey);

            return this;
        }

        public string GetConfigScript()
        {
            if (_configScript == null)
            {
                throw new InvalidOperationException("Config script has not been initialized. Use the appropriate constructor.");
            }
            return _configScript;
        }

        public SubscriberScript CreateConfigScript()
        {
            return new SubscriberScript
            {
                ConfigScript = _configScript
            };
        }

        public SubscriberScript CreateCommonScript()
        {
            var commonScripts = _bundleConfig.GetBundle("common");
            return new SubscriberScript
            {
                CommonScript = commonScripts
            };
        }
    }
}
