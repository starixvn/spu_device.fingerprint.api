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

        public ISubscriberScript CreateDirectSubscriberScript()
        {
            return new DirectSubscriberScript
            {
                ConfigScript = _configScript
            };
        }

        /// <summary>
        /// Creates a common subscriber script with predefined scripts.
        /// </summary>
        /// <returns>An instance of ISubscriberScript containing common scripts.</returns>
        public ISubscriberScript CreateSubscriberCommonScript()
        {
            var commonScripts = _bundleConfig.GetBundle("common");
            return new ApiSubscriberScript
            {
                CommonScript = commonScripts
            };
        }

        /// <summary>
        /// Creates a token-based subscriber script.
        /// </summary>
        /// <returns>An instance of TokenSubscriberScript with token scripts.</returns>
        public ISubscriberScript CreateSubscriberTokenScript()
        {
            var tokenScript = _scriptLoader.LoadTokenScript();

            return new TokenSubscriberScript
            {
                TokenScript = tokenScript
            };
        }
    }
}
