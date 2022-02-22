using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class UpdateTransactionNCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_RawTransJson")]
        public string Json { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_TransactionN_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
