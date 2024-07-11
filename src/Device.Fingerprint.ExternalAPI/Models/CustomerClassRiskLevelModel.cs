using System;

namespace Device.Fingerprint.ExternalAPI.Models
{
    public class CustomerClassRiskLevelModel
    {
        public long Id { get; set; }

        public string Account { get; set; }                               

        public string RiskLevel { get; set; }
    }
}
