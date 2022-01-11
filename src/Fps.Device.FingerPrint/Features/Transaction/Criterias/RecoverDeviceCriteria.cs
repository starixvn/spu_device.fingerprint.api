using Fanex.Data;
using Fanex.Data.Repository;

namespace Fps.Device.FingerPrint.Features.Transaction.Criterias
{
    public class RecoverDeviceCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_BatchSize")]
        public int Batch { get; set; }

        public override string GetSettingKey()
        {
            return "FP_RecoverDevice";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
