using System;
using System.Collections.Generic;
using System.Net;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class AssociationAccountUsageList
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public int AssociatedAccounts { get; set; }

        public IEnumerable<AssociationAccountUsageModel> AssociatedAccountDetails { get; set; }

        
    }
}
