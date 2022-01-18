using Newtonsoft.Json;

namespace Device.FingerPrint.Model
{
    public class ScriptConfigModel
    {
        [JsonProperty("a")]
        public string EncryptedKey { get; set; }

        [JsonProperty("b")]
        public string Token { get; set; }

        [JsonProperty("c")]
        public string SessionKey { get; set; }
    }
}
