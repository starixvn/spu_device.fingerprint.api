using System;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class AssociationDeviceModel
    {
        public string Device { get; set; }

        public DateTime FirstSeenDate { get; set; }
    }
}
