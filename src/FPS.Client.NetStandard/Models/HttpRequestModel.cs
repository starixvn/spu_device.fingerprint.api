using System;

namespace FPS.Client.NetStandard.Models
{
    public class HttpRequestModel
    {
        public string Account { get; set; }

        public string Subscriber { get; set; }

        public string Action { get; set; }

        public string ActionResult { get; set; }

        public string AcceptTypes { get; set; }

        public string Platform { get; set; }

        public string UserAgent { get; set; }

        public string BrowserScreen { get; set; }       

        public string Di { get; set; }

        public string Bi { get; set; }

        public string Token { get; set; }

        public string Url { get; set; }

        public string DebugInfo { get; set; }

        public string CaptchaResult { get; set; }

        public DateTime? LoginTime { get; set; }
    }
}
