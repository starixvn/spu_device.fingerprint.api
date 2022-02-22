using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class UpdateTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_RawTransJson")]
        public string Json { get; set; }

        [SpParam(SqlParamName = "ip_Retry")]
        public int Retry { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_Transaction_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
