using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.RawTransaction.Criterias
{
    public class InsertRawTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Transaction")]
        public string Transaction { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_RawTransaction_Insert";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
