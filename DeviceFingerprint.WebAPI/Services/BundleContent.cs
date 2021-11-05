namespace DeviceFingerprint.WebAPI.Services
{
    public class BundleContent
    {
        private readonly string scriptContent;
        private readonly IAppSettings appSettings;

        public BundleContent(string scriptContent, IAppSettings appSettings)
        {
            this.scriptContent = scriptContent;
            this.appSettings = appSettings;
        }

        public string Content
        {
            get
            {
                var JWTService = new JWT(appSettings);
                var script = this.scriptContent
                    .Replace("__key__", appSettings.EncryptPassword)
                    .Replace("__apiUrl__", $"{appSettings.ApiUrl}/api/fp")
                    .Replace("__token__", JWTService.GenerateJwtToken());

                return script;
            }
        }


    }
}
