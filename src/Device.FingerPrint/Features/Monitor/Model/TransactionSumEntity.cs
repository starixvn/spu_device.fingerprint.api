using System;

namespace Device.FingerPrint.Features.Monitor.Model
{
    public class TransactionSumEntity
    {
        public DateTime TransDate { get; set; }

        public int SubscriberId { get; set; }

        public int CountTrans { get; set; }
    }
}
