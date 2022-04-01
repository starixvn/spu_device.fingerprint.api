using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.TransformTransaction.Criterias
{
    public class TransformTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_key")]
        public int Key { get; set; }

        [SpParam(SqlParamName = "ip_delay")]
        public int Delay { get; set; }

        public override string GetSettingKey()
        {
            return "test_delay_sp";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
