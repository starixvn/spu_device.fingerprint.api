using Newtonsoft.Json;

namespace Device.FingerPrint.Model
{

    public class BrowserInfoModel : BrowserInfoProp
    {
        public string BrowserInfoCode { get; set; }
    }

    public class BrowserInfoProp
    {
        public string OSName { get; set; }

        public string BrowserName { get; set; }

        public decimal HeadlessScore { get; set; }

        public string HeadlessDetail { get; set; }

        public int Flagged { get; set; }
    }
}
