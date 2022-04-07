using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.SummaryTransaction.Criterias
{
    class SummaryTransactionMonthlyCriteria : NonQueryCommand
    {
        public override string GetSettingKey()
        {
            return "FPS_FP_Summary_ClientServerTransaction_Monthly";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
