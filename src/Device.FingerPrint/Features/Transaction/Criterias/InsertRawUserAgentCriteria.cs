using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class InsertRawUserAgentCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_RawUserAgentJson")]
        public string RawUserAgentJson { get; set; }

        [SpParam(Ignored = true)]
        public bool IsTransOfDBExtra { get; set; }

        public override string GetSettingKey()
        {
            if (this.IsTransOfDBExtra)
            {
                return "";
            }
            else
            {
                return "DCS_DC_Transform_RawUserAgent_Insert";
            }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
