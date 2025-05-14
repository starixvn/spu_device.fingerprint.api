
namespace Device.Fingerprint.InternalAPI.Model
{
    using Device.FingerPrint.Model;

    public class FPFingerPrintTransform
    {
        public ulong ID { get; set; }

        public string? Code { get; set; }

        public FPFingerprintAttributeModel? Attribute { get; set; }

        public DateTime InsertedTime { get; set; }
    }
}
