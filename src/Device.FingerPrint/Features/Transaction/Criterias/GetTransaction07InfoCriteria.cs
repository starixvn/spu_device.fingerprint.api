using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class GetTransaction07InfoCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Columns")]
        public string Columns { get; set; }

        [SpParam(SqlParamName = "ip_Conditions")]
        public string Conditions { get; set; }

        [SpParam(SqlParamName = "ip_BatchSize")]
        public uint BatchSize { get; set; }

        [SpParam(SqlParamName = "ip_PageNumber")]
        public uint PageNumber { get; set; }

        public override string GetSettingKey()
        {
            return "DCS_DC_Report_Transaction07_GetInfo";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
