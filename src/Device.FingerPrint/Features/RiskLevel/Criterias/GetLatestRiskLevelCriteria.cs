using Fanex.Data;
using Fanex.Data.Repository;
using System.Security.Permissions;

namespace Device.FingerPrint.Features.RiskLevel.Criterias
{
    public class GetLatestRiskLevelCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_SubscriberID")]
        public int SubscriberId { get; set; }

        [SpParam(SqlParamName = "ip_LastID")]
        public long LastId { get; set; }

        [SpParam(SqlParamName = "ip_BatchSize")]
        public int BatchSize { get; set; }

        public override string GetSettingKey()
        {
            return "CTS_DC_CustClassification_GetLatestRiskLevel";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
