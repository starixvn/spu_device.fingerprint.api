using System;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class AssociationAccountModel
    {
        public string Account { get; set; }

        public DateTime FirstAssociationDate { get; set; }
    }
}
