using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class UpdateTransactionNCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_RawTransJson")]
        public string RawTransJson { get; set; }

        [SpParam(SqlParamName = "ip_OffsetName")]
        public string OffsetName { get; set; }

        [SpParam(SqlParamName = "ip_Offsetvalue")]
        public long Offsetvalue { get; set; }

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
