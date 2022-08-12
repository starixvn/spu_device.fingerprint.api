using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.BrowserInfo.Criterias
{
    public class UpdateBrowserInfoFlaggedDCSCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BrowserInfoIdJson")]
        public string BrowserInfoIdJson { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_BrowserInfoFlaggedDCS_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
