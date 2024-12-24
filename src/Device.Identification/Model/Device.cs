using Device.Identification.JsonConverter;
using Newtonsoft.Json;

namespace Device.Identification.Model
{
    public class Device
    {
        public string ClientDeviceIds { get; set; }

        public string Code { get; set; }

        public string Navigator { get; set; }

        public string Plugins { get; set; }

        public string MimeTypes { get; set; }

        public string UserAgent { get; set; }

        public string Screen { get; set; }

        [JsonConverter(typeof(MonitorConverter))]
        public long? Monitor { get; set; }

        public string Fonts { get; set; }

        public int? TimeZone { get; set; }

        public string GPUCode { get; set; }

        public string WebGL { get; set; }

        public string Language { get; set; }

        public int? SystemTimeOffset { get; set; }

        public bool CookieEnabled { get; set; }

        public bool DoNotTrack { get; set; }

        public string SuperCookies { get; set; }

        public string Browser { get; set; }

        public short DeviceType { get; set; }

        public string Accept { get; set; }

        public string Headers { get; set; }

        [JsonIgnore]
        public DateTime? CreatedDate { get; set; }

        public string Id { get; set; }

        public ulong? IdRef { get; set; }

        [JsonIgnore]
        public ulong Increment { get; set; }
    }
}