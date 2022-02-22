using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class UpdateCookieCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Cookie")]
        public string Code { get; set; }

        [SpParam(SqlParamName = "op_Id")]
        public long Id { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_Cookie_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
