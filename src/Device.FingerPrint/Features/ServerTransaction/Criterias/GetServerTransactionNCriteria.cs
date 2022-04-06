using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.ServerTransaction.Criterias
{
    class GetServerTransactionNCriteria : CriteriaBase
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        [SpParam(SqlParamName = "ip_OffsetId")]
        public long OffsetId { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_ServerTransactionN_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
