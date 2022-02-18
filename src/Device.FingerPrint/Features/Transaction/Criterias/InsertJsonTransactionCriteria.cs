using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class InsertJsonTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Transaction")]
        public string Transaction { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_InsertTransaction";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
