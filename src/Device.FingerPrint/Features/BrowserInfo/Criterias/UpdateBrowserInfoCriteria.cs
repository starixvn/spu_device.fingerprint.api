using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.BrowserInfo.Criterias
{
    public class UpdateBrowserInfoCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BrowserInfo")]
        public string BrowserInfo { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_BrowserInfo_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
