using Device.FingerPrint.Model;

namespace Device.FingerPrint.PortalAPI.Models
{
    public class FingerprintParameter
    {
        public FingerprintModel fingerprint { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }

    }
}
