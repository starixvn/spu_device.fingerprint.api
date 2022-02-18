using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Subscriber.Criterias
{
    public class GetSubscriberInfoCriteria : CriteriaBase
    {
        [SpParam(SqlParamName = "ip_SubscriberId")]
        public int SubscriberId { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_GetSubscriberInfoById";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
