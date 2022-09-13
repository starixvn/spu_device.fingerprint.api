namespace Device.FingerPrint.Features.ClientTransaction.Model
{
    public class FingerprintAttributesEntity
    {
        public string FingerPrintCode { get; set; }

        public string Attribute { get; set; }

        public string FingerPrintAttributeCode { get; set; }

        public string DoNotTrack { get; set; }

        public string GLRenderer { get; set; }

        public string HeaderAccept { get; set; }

        public string HeaderAcceptEncoding { get; set; }

        public string Headers { get; set; }

        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }
    }
}
