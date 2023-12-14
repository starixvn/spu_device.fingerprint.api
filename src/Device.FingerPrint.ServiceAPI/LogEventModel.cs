namespace Device.FingerPrint.ServiceAPI
{
    using System;

    public class LogEventModel
    {
        public DateTime Timestamp { get; set; }

        public string Level { get; set; }

        public string RenderedMessage { get; set; }
    }
}
