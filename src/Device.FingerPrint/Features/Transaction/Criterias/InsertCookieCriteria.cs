using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class InsertCookieCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_CookieJson")]
        public string CookieJson { get; set; }

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
                return "DCS_DC_Transform_Cookie_Insert";
            }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
