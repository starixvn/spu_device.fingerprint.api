namespace Device.FingerPrint.Model
{
    public class HeaderModel
    {
        public string Accept { get; set; }

        public string Encoding { get; set; }

        public string Headers { get; set; }

        public string SecChUa { get; set; }

        public string SecChUaPlatform { get; set; }

        public string SecChUaMobile { get; set; }

        public string Url { get; set; }
    }

    public class ParseUserAgentModel
    {
        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }
    }
}
