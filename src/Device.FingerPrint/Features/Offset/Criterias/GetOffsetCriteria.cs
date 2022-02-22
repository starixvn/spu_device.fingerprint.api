using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Offset.Criterias
{
    class GetOffsetCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_OffsetName")]
        public string OffsetName { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Core_Offset_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
