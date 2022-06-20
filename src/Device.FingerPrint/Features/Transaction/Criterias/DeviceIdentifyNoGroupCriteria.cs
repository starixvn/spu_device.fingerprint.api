using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class DeviceIdentifyNoGroupCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_TransJson")]
        public string TransJson { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_DeviceIdentify_NoGroup";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
