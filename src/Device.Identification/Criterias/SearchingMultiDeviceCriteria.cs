using Fanex.Data.Repository;

namespace Device.Identification.Criterias
{
    public class SearchingMultiDeviceCriteria : CriteriaBase
    {
        public string DeviceIds { get; set; }

        public string DeviceCode { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_DI_MultiDeviceCheck";
        }

        public override bool IsValid()
        {
            return !string.IsNullOrEmpty(DeviceIds);
        }
    }
}