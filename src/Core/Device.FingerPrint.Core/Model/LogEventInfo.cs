namespace Device.FingerPrint.Core.Model
{
    using System;

    [Serializable]
    public class LogEventInfo
    {
        public string SubscriberName { get; set; }
        public DateTime EventTime { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Method { get; set; }
    }
}
