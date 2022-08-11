using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.RawUserAgent.Criterias
{
    public class GetRawUserAgentCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Detect_RawUserAgent_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
