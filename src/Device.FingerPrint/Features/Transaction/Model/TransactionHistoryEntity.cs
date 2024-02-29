using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class TransactionHistoryEntity
    {
        public DateTime TransTime { get; set; }

        public long TransID { get; set; }               

        public string FirstDeviceCode { get; set; }

        public string IP { get; set; }                

        public string RobotTracking { get; set; }

        public string Action { get; set; }

        public string ActionResult { get; set; }

        public string OS { get; set; }

        public string Browser { get; set; }

        public string UrlDetail { get; set; }

        public string UserName { get; set; }

        public string RegisterName { get; set; } 

        public int SubscriberID { get; set; }

        public long CTSCustID { get; set; }
    }
}
