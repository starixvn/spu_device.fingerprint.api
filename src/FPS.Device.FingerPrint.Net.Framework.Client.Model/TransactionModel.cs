using System;

namespace FPS.Device.FingerPrint.Net.Framework.Client.Model
{
    public class TransactionModel
    {
        public int SubscriberId { get; set; }

        public string LoginName { get; set; }        

        public string SessionKey { get; set; }

        public string ClientIp { get; set; }

        public string UserAgent { get; set; }

        public DateTime TransTime { get; set; }

        public string Url { get; set; }

        public string Action { get; set; }
    }
}
