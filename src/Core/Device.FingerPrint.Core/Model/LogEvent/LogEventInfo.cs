namespace Device.FingerPrint.Core.Model.LogEvent
{
    using System;

    public class LogEventInfo : LogEventEntity
    {
        public string LoginName { get; set; }

        public DateTime? TransTime { get; set; }

        public string EventCodeDetail { get; set; }
    }
}
