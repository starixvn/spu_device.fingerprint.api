using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.FingerPrint.Model
{
   public class FingerPrintEntity
    {
        public string FingerPrintCode { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }

        public string Attribute { get; set; }

        public string UserAgentAttribute { get; set; }

        public string UserAgentCode { get; set; }

        public string CookieCode { get; set; }

        public string Flagged { get; set; }

        public string Url { get; set; }
    }
}
