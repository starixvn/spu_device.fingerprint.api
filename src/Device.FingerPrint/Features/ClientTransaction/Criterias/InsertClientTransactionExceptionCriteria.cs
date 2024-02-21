using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.ClientTransaction.Criterias
{
    public class InsertClientTransactionExceptionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_ExceptionJson")]
        public string ExceptionJson { get; set; }

        public override string GetSettingKey()
        {
            return "CTS_Log_FPSClientTransactionException_Insert";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
