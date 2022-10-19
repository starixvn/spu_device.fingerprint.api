using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class GetTransactionAttributesEvolutionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_FingerPrintIds")]
        public string FingerPrintIds { get; set; }

        [SpParam(SqlParamName = "ip_BrowserInstanceCode")]
        public string BrowserInstanceCode { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Recover_TransactionEvolutionAttributes_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
