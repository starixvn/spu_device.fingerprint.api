using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Subscriber.Criterias
{
    public class GetSubscriberInfoByNameCriteria : CriteriaBase
    {
        [SpParam(SqlParamName = "ip_SubscriberName")]
        public string SubscriberName { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Core_SubscriberInfoByName_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
