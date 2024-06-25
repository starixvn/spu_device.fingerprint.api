using Device.FingerPrint.Features.Transaction.Model;
using DeviceDetectorNET;
using System;
using System.Collections.Generic;
using System.Net;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class TransactionHistoryList
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public int TotalTrans
        {
            get
            {
                return Trans.Count();
            }
        }

        public IEnumerable<TransactionHistoryModel> Trans { get; set; }    
    }
}
