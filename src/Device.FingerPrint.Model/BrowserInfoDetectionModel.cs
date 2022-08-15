namespace Device.FingerPrint.Model
{
    public class BrowserInfoDetectionModel
    {
        public long Id { get; set; }

        public long UserAgentId { get; set; }

        public string UserAgent { get; set; }

        public string UABrowserName { get; set; }

        public string UAOSName { get; set; }

        public string HeadlessScore { get; set; }

        public string BrowserName { get; set; }

        public string OSName { get; set; }

        public int Flagged { get; set; }
    }
}
