using Device.FingerPrint.Features.Transaction.Model;
using System.Collections.Generic;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class TransactionHistoryList: PagingModel
    {       
        public IEnumerable<TransactionHistoryModel> Trans { get; set; }
    }
}
