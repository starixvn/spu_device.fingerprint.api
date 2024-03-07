using Device.FingerPrint.Model;
using System;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class LatestTransactionEntity
    {
        public DateTime TransTime { get; set; }

        public long TransID { get; set; }

        public int SubscriberId { get; set; }

        public long CTSCustId { get; set; }

        public string RegisterName { get; set; }

        public string UserName { get; set; }

        public string FirstDeviceCode { get; set; }

        public string Action { get; set; }

        public string ActionResult { get; set; }

        public string OS { get; set; }

        public string Browser { get; set; }        

        public string UrlDetails { get; set; }        

        public string IP { get; set; }        

        public string RobotTracking { get; set; }        
    }
}
