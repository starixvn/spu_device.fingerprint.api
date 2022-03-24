using System;

namespace Device.FingerPrint.Model
{
    public class ServerTransMonthlySumModel
    {
        public DateTime TransMonth { get; set; }

        public int SubscriberId { get; set; }

        public string SubscriberName { get; set; }

        public int TotalTrans { get; set; }

        public int TotalValidTrans { get; set; }
    }
}
