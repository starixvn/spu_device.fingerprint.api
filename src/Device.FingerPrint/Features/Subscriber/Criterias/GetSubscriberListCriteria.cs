using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Subscriber.Criterias
{
    public class GetSubscriberListCriteria : CriteriaBase
    {
        public override string GetSettingKey()
        {
            return "FPS_FP_GetSubscriberList";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
