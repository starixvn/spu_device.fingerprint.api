namespace Device.FingerPrint.ServiceAPI
{
    using System;
    using Device.FingerPrint.Core.Model;

    public class LogEvent
    {
        public DateTime Timestamp { get; set; }

        public string Level { get; set; }

        public string RenderedMessage { get; set; }
    }
}
