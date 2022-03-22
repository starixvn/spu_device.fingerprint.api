using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Monitor.Criterias
{
    public class ServerTransDailySumCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_dmyDate")]
        public string DMYDate { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Monitor_ServerTransactionDailySum_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
