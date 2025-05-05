using Fanex.Data;
using Fanex.Data.Repository;
using System;

namespace Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class GetFPFingerPrintCriteria : CriteriaBase
    {
        [SpParam(SqlParamName = "ip_FromDate")]
        public DateTime FromDate { get; set; }

        [SpParam(SqlParamName = "ip_ToDate")]
        public DateTime ToDate { get; set; }

        [SpParam(SqlParamName = "ip_BatchSize")]
        public int BatchSize { get; set; }

        [SpParam(SqlParamName = "ip_LastID")]
        public ulong LastID { get; set; }

        public override string GetSettingKey()
        {
            return "DCS_DC_OpenSearch_FPFingerPrint_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}