using System;
using System.Collections.Generic;
using System.Net;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class AssociationAccountUsageList
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public int AssociatedAccounts { get; set; }

        public IEnumerable<AssociationAccountUsageModel> AssociatedAccountDetails { get; set; }

        
    }
}
