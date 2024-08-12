namespace Device.FingerPrint.Model
{
    public class FPNewFingerprintModel
    {
        public string Code { get; set; }

        public FPFingerprintAttributeModel AttributeModel { get; set; }
    }

    public class FPFingerprintExtraModel
    {
        public string Code { get; set; }

        public FPFingerprintAttributeExtraModel AttributeModel { get; set; }
    }
}
