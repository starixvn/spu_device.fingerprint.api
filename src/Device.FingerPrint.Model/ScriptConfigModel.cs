using Newtonsoft.Json;

namespace Device.FingerPrint.Model
{
    public class ScriptConfigModel
    {
        [JsonProperty("ek")]
        public string EncryptedKey { get; set; }

        [JsonProperty("tk")]
        public string Token { get; set; }

        [JsonProperty("iv")]
        public string IV { get; set; }

        [JsonProperty("sa")]
        public string Salt { get; set; }
    }
}
