using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.BrowserInfo.Criterias
{
    public class GetBrowserInfoFlaggedDCSCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_BrowserInfoFlaggedDCS_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
