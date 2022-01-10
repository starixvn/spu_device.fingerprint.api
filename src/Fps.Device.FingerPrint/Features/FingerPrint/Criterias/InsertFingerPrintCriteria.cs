using Fanex.Data;
using Fanex.Data.Repository;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class InsertFingerPrintCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_FingerPrint")]
        public string FingerPrint { get; set; }

        [SpParam(SqlParamName = "ip_SessionKey")]
        public string SessionKey { get; set; }

        [SpParam(SqlParamName = "ip_SubscriberId")]
        public int SubscriberId { get; set; }

        [SpParam(SqlParamName = "ip_Attributes")]
        public string Attributes { get; set; }

        [SpParam(SqlParamName = "ip_Device")]
        public string Device { get; set; }

        [SpParam(SqlParamName = "ip_DeviceCode")]
        public string DeviceCode { get; set; }

        [SpParam(SqlParamName = "ip_CookieId")]
        public long CookieId { get; set; }


        public override string GetSettingKey()
        {
            return "FP_InsertFingerPrint";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
