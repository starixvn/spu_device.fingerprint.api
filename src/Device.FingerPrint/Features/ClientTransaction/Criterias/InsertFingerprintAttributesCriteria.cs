using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.ClientTransaction.Criterias
{
    public class InsertFingerprintAttributesCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_FingerprintAttributes")]
        public string FingerprintAttributes { get; set; }

        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_FingerprintAttributes_Insert";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
