using System;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class DeviceTransactionEntity
    {
        public long Id { get; set; }

        public long RawTransId { get; set; }

        public long CustId { get; set; }

        public long SubAccId { get; set; }

        public string LoginName { get; set; }

        public int SubscriberId { get; set; }

        public string FingerPrintCode { get; set; }

        public long CookieId { get; set; }

        public long DeviceId { get; set; }

        public DateTime TransTime { get; set; }

        public string ClientIp { get; set; }

        public string UserAgent { get; set; }

        public string Url { get; set; }

        public string Action { get; set; }

        public DateTime ModifiedDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public string TransKey { get; set; }

        public string Flagged { get; set; }
    }
}
