namespace Device.FingerPrint.Model
{
    public class FingerprintAttributeModel
    {
        public string FingerPrintCode { get; set; }

        public string Attribute { get; set; }

        public string Accept { get; set; }

        public string Encoding { get; set; }

        public string Headers { get; set; }

        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }

        public string UserAgent { get; set; }

        public string Platform { get; set; }

        public string BrowserInstanceCode { get; set; }
    }
}
