using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.ServerTransaction.Criterias
{
    class UpdateServerTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_RawTransJson")]
        public string RawTransJson { get; set; }

        [SpParam(SqlParamName = "ip_Retry")]
        public int Retry { get; set; }

        [SpParam(SqlParamName = "ip_OffsetId")]
        public int OffsetId { get; set; }

        [SpParam(SqlParamName = "ip_OffsetValue")]
        public long OffsetValue { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_ServerTransaction_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
