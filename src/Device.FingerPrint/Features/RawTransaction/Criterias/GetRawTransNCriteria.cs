using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.RawTransaction.Criterias
{
    class GetRawTransNCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Size { get; set; }

        [SpParam(SqlParamName = "ip_Offset")]
        public long Offset { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_GetRawTransN";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
