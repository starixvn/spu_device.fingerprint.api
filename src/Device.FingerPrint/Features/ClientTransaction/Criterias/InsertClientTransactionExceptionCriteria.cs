using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.ClientTransaction.Criterias
{
    public class InsertClientTransactionExceptionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Function")]
        public string Function { get; set; }

        [SpParam(SqlParamName = "ip_Exception")]
        public string Exception { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_ClientTransaction_LogsException_Insert";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
