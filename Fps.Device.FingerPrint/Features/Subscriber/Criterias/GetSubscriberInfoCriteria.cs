using Fanex.Data;
using Fanex.Data.Repository;

namespace Fps.Device.FingerPrint.Features.Subscriber.Criterias
{
    public class GetSubscriberInfoCriteria : CriteriaBase
    {
        [SpParam(SqlParamName = "ip_SubscriberId")]
        public int SubscriberId { get; set; }

        public override string GetSettingKey()
        {
            return "CTS_AD_Subscriber_GetInfoBySubscriberId";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
