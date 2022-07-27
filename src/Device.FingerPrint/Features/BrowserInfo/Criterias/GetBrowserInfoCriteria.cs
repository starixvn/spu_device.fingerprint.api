using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.BrowserInfo.Criterias
{
    public class GetBrowserInfoCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        [SpParam(SqlParamName = "ip_OffsetId")]
        public long OffsetId { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_BrowserInfo_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
