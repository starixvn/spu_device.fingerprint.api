
namespace Device.Fingerprint.InternalAPI.Model
{
    using Device.FingerPrint.Features.FingerPrint.Entities;
    public class FPFingerprintLogstashModel
    {
        public FPFingerPrintEntity Message { get; set; }

        public string Tags { get; set; }
    }
}
