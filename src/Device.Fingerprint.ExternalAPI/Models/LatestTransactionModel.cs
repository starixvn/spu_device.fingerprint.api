using System;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class LatestTransactionModel
    {
        public DateTime TransTime { get; set; }

        public long TransID { get; set; }        

        public string RegisterName { get; set; }

        public string UserName { get; set; }

        public string Device { get; set; }

        public string Action { get; set; }                

        public string OS { get; set; }

        public string Browser { get; set; }

        public string Domain { get; set; }

        public string IP { get; set; }

        public long RobotTracking { get; set; }
    }
}
