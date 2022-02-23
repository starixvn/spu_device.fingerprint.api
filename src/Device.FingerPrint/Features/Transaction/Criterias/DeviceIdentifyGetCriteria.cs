using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class DeviceIdentifyGetCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        [SpParam(SqlParamName = "ip_OffsetName")]
        public string OffsetName { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_DeviceIdentify_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
