using Device.FingerPrint.Features.Transaction.Model;
using System.Collections.Generic;
using System.Net;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class CustomerClassRiskLevelList
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }
       
        public IEnumerable<CustomerClassRiskLevelModel> Trans { get; set; }
    }
}
