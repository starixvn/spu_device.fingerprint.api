namespace Device.FingerPrint.Core.Model
{
    using System;

    public class LogEventInfo
    {
        public DateTime Date { get; set; }

        public int SubscriberID { get; set; }

        public string Source { get; set; }

        public string Destination { get; set; }

        public string Method { get; set; }

        public string SystemName { get; set; } = "test system";
    }
}
