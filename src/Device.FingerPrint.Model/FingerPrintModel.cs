
namespace Device.FingerPrint.Model
{
    public class FingerPrintModel
    {
        public string FingerPrint { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }

        public string Attributes { get; set; }

        public string DeviceInfo { get; set; }

        public string DeviceCode { get; set; }

        public long CookieId { get; set; }

        public string Flagged { get; set; }
    }
}
