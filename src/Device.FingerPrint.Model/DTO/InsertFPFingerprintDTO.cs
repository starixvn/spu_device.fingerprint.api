namespace Device.FingerPrint.Model.DTO
{
    public class InsertFPFingerprintDTO
    {
        public string FPFingerPrintCode { get; set; }

        public FPFingerprintAttributeModel FPFingerPrintAttribute { get; set; }

        public string FPFingerPrintExtraCode { get; set; }

        public FPFingerprintAttributeExtraModel FPFingerPrintExtraAttribute { get; set; }

        public string FPFingerPrintOldCode { get; set; }
    }
}
