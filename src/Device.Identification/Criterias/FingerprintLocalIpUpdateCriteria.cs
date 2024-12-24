namespace Device.Identification.Criterias
{
    using Fanex.Data.Repository;

    public class FingerprintLocalIpUpdateCriteria : CriteriaBase
    {
        public string DeviceId { get; set; }

        public string Ip { get; set; }

        public string LocalIps { get; set; }

        public string Error { get; set; }

        public string Message { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_DI_LocalIpUpdate_FOR_FP";
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(DeviceId)
                && !string.IsNullOrEmpty(Ip)
                && !string.IsNullOrEmpty(LocalIps);
        }
    }
}