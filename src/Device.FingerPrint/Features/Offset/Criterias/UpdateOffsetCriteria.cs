using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Offset.Criterias
{
    class UpdateOffsetCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_OffsetName")]
        public string OffsetName { get; set; }

        [SpParam(SqlParamName = "ip_OffsetValue")]
        public long OffsetValue { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Offset_Offset_UpdateOffsetValue";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
