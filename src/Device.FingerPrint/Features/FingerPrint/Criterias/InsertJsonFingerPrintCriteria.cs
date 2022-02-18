using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class InsertJsonFingerPrintCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_FingerPrint")]
        public string FingerPrint { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_InsertFingerPrint";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
