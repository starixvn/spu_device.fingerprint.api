using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.UserAgent.Criterias
{
    public class GetUserAgentCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        [SpParam(SqlParamName = "ip_OffsetId")]
        public long OffsetId { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Detect_UserAgent_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
