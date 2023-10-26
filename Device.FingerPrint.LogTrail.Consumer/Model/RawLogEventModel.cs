using System;

namespace Device.FingerPrint.LogTrail.Model
{
    public class RawLogEventModel
    {
        public DateTime Date { get; set; }

        public int SubscriberID { get; set; }

        public string Source { get; set; }

        public string Method { get; set; }

        public string SystemName { get; set; }

        public int Total { get; set; }
    }
}
