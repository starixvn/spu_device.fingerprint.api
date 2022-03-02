using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.RawTransaction.Criterias
{
    class GetRawTransactionNCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        [SpParam(SqlParamName = "ip_OffsetId")]
        public int OffsetId { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_RawTransactionN_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
