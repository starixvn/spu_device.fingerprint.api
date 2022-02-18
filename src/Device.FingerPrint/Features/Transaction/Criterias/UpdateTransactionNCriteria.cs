using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class UpdateTransactionNCriteria : NonQueryCommand
    {
        public override string GetSettingKey()
        {
            return "FPS_FP_UpdateTransactionN";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
