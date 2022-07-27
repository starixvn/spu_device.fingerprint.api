using System;

namespace Device.FingerPrint.Model
{
    public class TransactionDTO
    {
        public string SubscriberName { get; set; }

        public string LoginName { get; set; }

        public string SessionKey { get; set; }

        public string ClientIp { get; set; }

        public string Url { get; set; }

        public string Action { get; set; }

        public string TransKey { get; set; }

        public long CustId { get; set; }

        public long SubAccId { get; set; }

        public DateTime TransTime { get; set; }
    }
}
