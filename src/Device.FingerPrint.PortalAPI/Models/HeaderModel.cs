namespace Device.FingerPrint.PortalAPI.Models
{
    public class HeaderModel
    {
        public string Accept { get; set; }

        public string Encoding { get; set; }

        public string Headers { get; set; }

        public string UserAgent { get; set; }

        public string secChUa { get; set; }

        public string secChUaPlatform { get; set; }

        public string secChUaMobile { get; set; }
    }

    public class ParseUserAgentModel
    {
        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }
    }
}
