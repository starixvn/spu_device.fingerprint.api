using System;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class RawTransactionEntity
    {
        public long Id { get; set; }

        public string SessionKey { get; set; }

        public long CustId { get; set; }

        public long SubAccId { get; set; }

        public string LoginName { get; set; }

        public int SubscriberId { get; set; }

        public DateTime Transtime { get; set; }

        public string ClientIP { get; set; }

        public string UserAgent { get; set; }

        public string Url { get; set; }

        public string TransKey { get; set; }

        public string Action { get; set; }

        public int? Retry { get; set; }
    }
}
