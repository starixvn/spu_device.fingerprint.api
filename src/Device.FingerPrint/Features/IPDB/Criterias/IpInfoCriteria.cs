using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.IPDB.Criterias
{
    internal class IpInfoCriteria : CriteriaBase
    {
        [SpParam(SqlParamName = "IPs")]
        public string IpAddressList { get; set; }

        public override string GetSettingKey()
        {
            return "GetLocationByIPList";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
