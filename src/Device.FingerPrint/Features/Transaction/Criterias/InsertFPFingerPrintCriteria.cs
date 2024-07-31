using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class InsertFPFingerPrintCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_FingerPrintJson")]
        public string FingerPrintJson { get; set; }

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
                return "DCS_DC_Transform_FP_FingerPrint_Insert";
            }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
