using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class GetFingerPrintAttributesEvolutionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_FingerPrintIds")]
        public string FingerPrintIds { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Recover_FingerPrintAttributesEvolution_Get";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
