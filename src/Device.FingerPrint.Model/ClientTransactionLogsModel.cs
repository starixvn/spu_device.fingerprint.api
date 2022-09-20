using System;

namespace Device.FingerPrint.Model
{
    public class ClientTransactionLogsModel
    {
        public string SessionKey { get; set; }

        public int SubscriberId { get; set; }        

        public string Url { get; set; }

        public string UserAgent { get; set; }

        public DateTime TransTime { get; set; }
    }
}
