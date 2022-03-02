using System;

namespace Device.FingerPrint.Features.RawTransaction.Model
{
   public class TransactionEntity
    {
        public string LoginName { get; set; }

        public int SubscriberId { get; set; }

        public string SessionKey { get; set; }

        public DateTime TransTime { get; set; }

        public string ClientIp { get; set; }

        public string UserAgent { get; set; }

        public string Url { get; set; }

        public string Action { get; set; }

        public string TransKey { get; set; }

        public long CustId { get; set; }

        public long SubAccId { get; set; }

    }
}
