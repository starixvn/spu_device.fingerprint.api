using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class GetTransactionInfoCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_LastTransID")]
        public ulong LastTransID { get; set; }

        [SpParam(SqlParamName = "ip_BatchSize")]
        public int BatchSize { get; set; }


        public override string GetSettingKey()
        {
            return "DCS_DC_SAI_GetTransactionInfo";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
