using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.RawTransaction.Criterias
{
    class GetRawTransCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        [SpParam(SqlParamName = "ip_Offset")]
        public long Offset { get; set; }

        [SpParam(SqlParamName = "ip_Retry")]
        public int Retry { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_GetRawTrans";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
