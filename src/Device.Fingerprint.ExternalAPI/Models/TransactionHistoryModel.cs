using System;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class TransactionHistoryModel
    {
        public DateTime DateTime { get; set; }

        public long TrackingNumber { get; set; }

        public string Account { get; set; }

        public string RiskLevel { get; set; }

        public string Action { get; set; }

        public string Device { get; set; }

        public string DeviceType { get; set; }

        public string Browser { get; set; }

        public string Domain { get; set; }

        public string IP { get; set; }

        public string RobotTracking { get; set; }    

    }
}
