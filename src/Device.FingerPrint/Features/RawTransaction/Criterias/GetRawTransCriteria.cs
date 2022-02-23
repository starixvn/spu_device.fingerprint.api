using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.RawTransaction.Criterias
{
    class GetRawTransCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        [SpParam(SqlParamName = "ip_OffsetName")]
        public string OffsetName { get; set; }

        [SpParam(SqlParamName = "ip_Retry")]
        public int Retry { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_RawTrans_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
