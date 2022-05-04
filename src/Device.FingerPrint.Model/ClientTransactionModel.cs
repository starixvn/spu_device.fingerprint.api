using System;
using System.Collections.Generic;

namespace Device.FingerPrint.Model
{
    public class ClientTransactionModel
    {
        public string FingerPrintCode { get; set; }

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }

        public string Attribute { get; set; }

        public string CookieCode { get; set; }

        public string Flagged { get; set; }

        public string Url { get; set; }

        public DateTime TransTime { get; set; }
    }
}
