using System.Net;

namespace Device.Fingerprint.InternalAPI.Model
{
    public class TransactionInfoResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public IEnumerable<TransactionInfoModel> TransactionInfo { get; set; }
    }
}
