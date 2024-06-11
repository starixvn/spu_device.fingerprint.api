using System;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class AssociationModel
    {
        public long AssociationID { get; set; }

        public long DeviceID { get; set; }

        public string DeviceCode { get; set; }

        public long AccountID { get; set; }

        public string Account { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
