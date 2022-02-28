using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class MergeTransactionNewCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_TransJson")]
        public string TransJson { get; set; }


        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_Transaction_Merge_NEW";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
