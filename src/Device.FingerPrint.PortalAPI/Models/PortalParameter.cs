using System.Text.Json.Serialization;

namespace Device.FingerPrint.PortalAPI.Models
{
    public class PortalParameter
    {
        [JsonPropertyName("dt")]
        public string Data { get; set; }

        [JsonPropertyName("sk")]
        public string SessionKey { get; set; }

        [JsonPropertyName("sv")]
        public string Version { get; set; }
    }
}
