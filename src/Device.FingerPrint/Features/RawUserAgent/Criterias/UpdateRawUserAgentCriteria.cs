using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.RawUserAgent.Criterias
{
    class UpdateRawUserAgentCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_RawUserAgentJson")]
        public string RawUserAgentJson { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Detect_RawUserAgent_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
