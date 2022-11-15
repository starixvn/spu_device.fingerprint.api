using Device.FingerPrint.Model;

namespace Device.FingerPrint.PortalAPI.Models
{
    public class BrowserDetectParameter
    {
        public BrowserDetectModel BrowserDetect { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }
    }
}
