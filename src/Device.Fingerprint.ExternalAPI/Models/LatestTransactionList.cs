using System.Collections.Generic;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class LatestTransactionList: PagingModel
    {     
        public IEnumerable<LatestTransactionModel> Trans { get; set; }
    }
}
