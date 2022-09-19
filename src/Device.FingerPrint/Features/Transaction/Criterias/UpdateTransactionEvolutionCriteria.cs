using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class UpdateTransactionEvolutionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_TransJson")]
        public string Transaction { get; set; }

        [SpParam(SqlParamName = "ip_Type")]
        public int Type { get; set; }


        public override string GetSettingKey()
        {
            return "FPS_FP_Recover_TransactionEvolution_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}