using System;

namespace Device.FingerPrint.Model
{
    public class ServerTransactionModel
    {
        public long Id { get; set; }

        public string SessionKey { get; set; }

        public long CustId { get; set; }

        public long SubAccId { get; set; }

        public ulong AccountId { get; set; }

        public string LoginName { get; set; }

        public int SubscriberId { get; set; }

        public DateTime TransTime { get; set; }

        public string ClientIP { get; set; }

        public ulong UserAgentId { get; set; }

        public string Url { get; set; }

        public string TransKey { get; set; }

        public string Action { get; set; }

        public int? Retry { get; set; }
    }
}
