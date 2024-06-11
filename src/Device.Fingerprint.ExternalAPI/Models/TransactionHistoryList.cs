using Device.FingerPrint.Features.Transaction.Model;
using System;
using System.Collections.Generic;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class TransactionHistoryList
    {       
        public IEnumerable<TransactionHistoryModel> Trans { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }
    }
}
