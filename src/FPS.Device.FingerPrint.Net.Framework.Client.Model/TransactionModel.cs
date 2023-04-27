
namespace FPS.Device.FingerPrint.Net.Framework.Client.Model
{
    public class TransactionModel
    {
        public string SubscriberName { get; set; }

        public int SubscriberId { get; set; }

        public string LoginName { get; set; }        

        public string SessionKey { get; set; }

        public string ClientIp { get; set; }

        public string UserAgent { get; set; }        

        public string Url { get; set; }

        public string Action { get; set; }

        public string TransKey { get; set; }

        public long CustId { get; set; }

        public long SubAccId { get; set; }
    }
}
