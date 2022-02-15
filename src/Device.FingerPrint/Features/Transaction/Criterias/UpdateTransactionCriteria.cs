using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class UpdateTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }


        public override string GetSettingKey()
        {
            return "FP_UpdateTransaction";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
