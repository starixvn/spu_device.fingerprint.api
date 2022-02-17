using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.FingerPrint.Model
{
   public class FingerPrintEntity
    {
        public string FingerPrint { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }

        public string Attributes { get; set; }

        public string DeviceInfo { get; set; }

        public string DeviceCode { get; set; }

        public long CookieId { get; set; }

        public string Flagged { get; set; }

        public string Url { get; set; }
    }
}
