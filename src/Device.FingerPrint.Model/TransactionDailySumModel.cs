using System;

namespace Device.FingerPrint.Model
{
    public class TransactionDailySumModel
    {
        public string SessionKey { get; set; }

        public DateTime TransDate { get; set; }

        public int SubscriberId { get; set; }

        public int TotalTrans { get; set; }
    }
}
