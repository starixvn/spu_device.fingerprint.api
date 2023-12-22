namespace Device.FingerPrint.Core.Model.LogEvent
{
    using System;

    public class LogEventEntity
    {
        public DateTime EventDate { get; set; }

        public int SubscriberID { get; set; }

        public string EventCode { get; set; }

        public string SystemName { get; set; }

        public string Source { get; set; }

        public string Destination { get; set; }

        public string Method { get; set; }

        public int Total { get; set; }
    }
}
