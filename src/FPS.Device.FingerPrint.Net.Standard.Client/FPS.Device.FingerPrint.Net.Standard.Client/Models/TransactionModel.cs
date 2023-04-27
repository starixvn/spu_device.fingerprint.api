
using Newtonsoft.Json;
using System;

namespace FPS.Device.FingerPrint.Net.Standard.Client.Model
{
    public class TransactionModel
    {
        [JsonProperty]
        internal string SubscriberName { get; set; }

        public string LoginName { get; set; }

        public string SessionKey { get; set; }

        public string ClientIp { get; set; }

        public string UserAgent { get; set; }

        public string Url { get; set; }

        public string Action { get; set; }

        [JsonProperty]
        internal string TransKey { get; set; }

        public long CustId { get; set; }

        public long SubAccId { get; set; }

        public DateTime LoginTime { get; set; }
    }
}
