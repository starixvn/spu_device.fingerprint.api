using System.Collections.Generic;
using System.Net;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class AssociationList
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ErrorMessage { get; set; }

        public int AssociatedDevices { get; set; }

        public IEnumerable<AssociationDeviceModel> AssociatedDeviceDetails { get; set; }

        public int AssociatedAccounts { get; set; }

        public IEnumerable<AssociationAccountModel> AssociatedAccountDetails { get; set; }
    }
}