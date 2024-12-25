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

        /// <summary>
        /// Creates a subscriber script based on the API action provided.
        /// </summary>
        /// <param name="apiAction">API action string to determine the subscriber script type.</param>
        /// <returns>An instance of ISubscriberScript.</returns>
        public ISubscriberScript CreateSubscriberScript(string apiAction)
        {
            if (IsApiSubscriber(apiAction))
            {
                return CreateApiSubscriberScript(apiAction);
            }

            return CreateDirectSubscriberScript();
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

        /// <summary>
        /// Determines if the provided API action corresponds to an API subscriber.
        /// </summary>
        /// <param name="apiAction">API action string.</param>
        /// <returns>True if it is an API subscriber; otherwise, false.</returns>
        private static bool IsApiSubscriber(string apiAction)
        {
            return !string.IsNullOrEmpty(apiAction);
        }

        /// <summary>
        /// Creates an API-specific subscriber script.
        /// </summary>
        /// <param name="apiAction">API action string to load the API script.</param>
        /// <returns>An instance of ApiSubscriberScript.</returns>
        private ISubscriberScript CreateApiSubscriberScript(string apiAction)
        {
            var apiScript = _scriptLoader.LoadApiScript(apiAction);
            return new ApiSubscriberScript
            {
                ConfigScript = _configScript,
                ApiScript = apiScript
            };
        }

        /// <summary>
        /// Creates a direct subscriber script with configuration scripts.
        /// </summary>
        /// <returns>An instance of DirectSubscriberScript.</returns>
        private ISubscriberScript CreateDirectSubscriberScript()
        {
            return new DirectSubscriberScript
            {
                ConfigScript = _configScript
            };
        }
    }
}
