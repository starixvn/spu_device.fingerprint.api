using System;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class LatestTransactionModel
    {
        public DateTime TransTime { get; set; }

        public long TrackingNumber { get; set; }        

        public string Account { get; set; }

        public string Action { get; set; }

        public string Device { get; set; }

        public string DeviceType { get; set; }

        public string Browser { get; set; }

        public string Domain { get; set; }

        public string IP { get; set; }

        public string ScreenSize { get; set; }        

        public string RobotTracking { get; set; }
    }
}
