namespace DeviceFingerprint.WebAPI.Services
{
    public class BundleContent
    {
        private readonly string scriptContent;
        private readonly IAppSettings appSetting;

        public BundleContent(string scriptContent, IAppSettings appSetting)
        {
            this.scriptContent = scriptContent;
            this.appSetting = appSetting;
        }

        public string Content
        {
            get
            {
                var script = this.scriptContent
                    .Replace("__key__", appSetting.EncryptPassword)
                    .Replace("__apiUrl__", $"{appSetting.ApiUrl}/api/fp")
                    .Replace("__token__", "");

                return script;
            }
        }


    }
}
