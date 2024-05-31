using Quartz;

namespace Device.FingerPrint.JobManagement.Models
{
    public class Trigger
    {
        public string TriggerType { get; set; }

        public string TriggerName { get; set; }

        public string CronExpression { get; set; }

        public int IntervalInSecond { get; set; }

        public string TimeZone { get; set; }

        public bool IgnoreMonitoring { get; set; }

        public int Interval { get; set; }

        public IntervalUnit IntervalUnit { get; set; }
    }

}
