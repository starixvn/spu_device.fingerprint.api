using Fanex.Data;
using Fanex.Data.Repository;

namespace Fps.Device.FingerPrint.Features.Transaction.Criterias
{
    public class InsertJsonTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Transaction")]
        public string Transaction { get; set; }

        public override string GetSettingKey()
        {
            return "FP_InsertTransaction_New";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
