namespace Device.FingerPrint.Core.Model.LogEvent
{
    using System;

    public class LogEventDetail : LogEventEntity
    {
        public string LoginName { get; set; }

        public DateTime? TransTime { get; set; }
    }
}
