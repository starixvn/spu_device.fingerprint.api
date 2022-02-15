using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class UpdateTransaction48Criteria : NonQueryCommand
    {
        public override string GetSettingKey()
        {
            return "FP_UpdateTransaction48";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
