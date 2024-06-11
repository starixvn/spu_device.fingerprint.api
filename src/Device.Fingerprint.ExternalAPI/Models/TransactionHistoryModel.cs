using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class TransactionHistoryModel
    {
        public DateTime DateTime { get; set; }

        public long TrackingNumber { get; set; }

        public string Device { get; set; }

        public string IP { get; set; }

        public string RobotTracking { get; set; }

        public string Action { get; set; }

        public string DeviceType { get; set; }

        public string Browser { get; set; }

        public string Domain { get; set; }

        public string Account { get; set; }

        public int SubscriberID { get; set; }

        public long RawFingerPrintID { get; set; }

        public long RawUserAgentID { get; set; }

    }
}
