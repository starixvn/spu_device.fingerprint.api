namespace FPS.Device.FingerPrint.LogTrail.Net.Standard.Client.Models
{
    using System;
    public class LogEventInfo
    {
        public DateTime EventDate { get; set; }

        public int SubscriberID { get; set; }

        public string SystemName { get; set; }

        public string Source { get; set; }

        public string Destination { get; set; }

        public string Method { get; set; }

        public int Total { get; set; }
    }
}
