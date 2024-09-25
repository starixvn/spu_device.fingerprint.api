using System;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class AssociationAccountUsageModel
    {
        public string Account { get; set; }

        public DateTime MinUsageDate { get; set; }

        public DateTime MaxUsageDate { get; set; }

        public int UsageCount { get; set; }
    }
}
