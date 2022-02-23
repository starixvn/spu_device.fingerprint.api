using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class DeviceIdentifyCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_TransJson")]
        public string TransJson { get; set; }

        [SpParam(SqlParamName = "ip_OffsetName")]
        public string OffsetName { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_DeviceIdentify";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
