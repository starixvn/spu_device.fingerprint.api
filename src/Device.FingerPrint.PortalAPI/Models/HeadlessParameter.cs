using Device.FingerPrint.Model;

namespace Device.FingerPrint.PortalAPI.Models
{
    public class HeadlessParameter
    {
        public HeadlessBrowserModel HeadlessBrowser { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }
    }
}
