using System;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class AssociationAccountUsageModel
    {
        public string Account { get; set; }

        public string MinUsageDate { get; set; }

        public string MaxUsageDate { get; set; }

        public int UsageCount { get; set; }
    }
}
