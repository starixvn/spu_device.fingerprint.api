using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class InsertRawFingerPrintCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_RawFingerPrintJson")]
        public string RawFingerPrintJson { get; set; }

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
                return "DCS_DC_Transform_RawFingerPrint_Insert";
            }
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
