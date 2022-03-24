using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Monitor.Criterias
{
    public class MonthlySumCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Month")]
        public string Month { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Report_Monthly_Sum";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
