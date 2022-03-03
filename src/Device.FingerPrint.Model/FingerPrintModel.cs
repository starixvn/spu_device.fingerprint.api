
namespace Device.FingerPrint.Model
{
    public class FingerPrintModel
    {
        public string FingerPrint { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }

        public string Attributes { get; set; }

        public string UserAgent { get; set; }

        public string UserAgentCode { get; set; }

        public string CookieCode { get; set; }

        public string Flagged { get; set; }

        public string Url { get; set; }
    }
}
