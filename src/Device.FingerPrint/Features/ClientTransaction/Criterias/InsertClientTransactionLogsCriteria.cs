using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.ClientTransaction.Criterias
{
    public class InsertClientTransactionLogsCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_FingerPrint")]
        public string FingerPrint { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_ClientTransaction_Logs_Insert";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
