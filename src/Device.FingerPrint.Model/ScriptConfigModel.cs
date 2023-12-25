namespace Device.FingerPrint.Model
{
    using System.Text.Json.Serialization;

    public class ScriptConfigModel
    {
        [JsonPropertyName("ek")]
        public string EncryptedKey { get; set; }

        [JsonPropertyName("tk")]
        public string Token { get; set; }

        [JsonPropertyName("iv")]
        public string IV { get; set; }

        [JsonPropertyName("sa")]
        public string Salt { get; set; }

        [JsonPropertyName("acpt")]
        public string Accept { get; set; }

        [JsonPropertyName("enc")]
        public string Encoding { get; set; }

        [JsonPropertyName("hk")]
        public string HeaderKeys { get; set; }

    }
}
