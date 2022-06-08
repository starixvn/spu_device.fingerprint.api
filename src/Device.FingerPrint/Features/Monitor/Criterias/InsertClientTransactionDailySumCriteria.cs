using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Monitor.Criterias
{
    public class InsertClientTransactionDailySumCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Data")]
        public string Data { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Monitor_ClientTransactionDailySum_Insert";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
