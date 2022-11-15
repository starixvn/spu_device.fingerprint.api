namespace Device.FingerPrint.PortalAPI.Models
{
    public class HeaderModel
    {
        public string Accept { get; set; }

        public string Encoding { get; set; }

        public string Headers { get; set; }

        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public string BrowserVersion { get; set; }

        public string UserAgent { get; set; }
    }
}
