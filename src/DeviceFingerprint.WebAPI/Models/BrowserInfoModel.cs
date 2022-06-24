using Newtonsoft.Json;

namespace Device.Fingerprint.WebAPI.Models
{
    public class BrowserInfoModel
    {
        public string ClientIp { get; set; }

        public string OSName { get; set; }

        public string OSVersion { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }
    }

    public class BrowserInfoJsonModel
    {

        [JsonProperty("fk")]
        public string SessionKey { get; set; }

        [JsonProperty("ua")]
        public string UserAgent { get; set; }

        [JsonProperty("uad")]
        public string UserAgentData { get; set; }

        [JsonProperty("binfo")]
        public string BrowserInfo { get; set; }

        [JsonProperty("hscore")]
        public string HeadlessScore { get; set; }
    }
}
