using Newtonsoft.Json;

namespace FPS.Device.FingerPrint.Model
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
