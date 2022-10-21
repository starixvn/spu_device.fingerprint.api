using System;

namespace Device.FingerPrint.Features.ClientTransaction.Model
{
    public class ClientTransactionLogsEntity
    {       

        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }

        public string Url { get; set; }

        public string UserAgent { get; set; }

        public DateTime TransTime { get; set; }

        public string Information { get; set; }
    }
}
