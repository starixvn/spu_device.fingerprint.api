using Fanex.Data;
using Fanex.Data.Repository;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class UpdateTransaction24Criteria : NonQueryCommand
    {
        public override string GetSettingKey()
        {
            return "FP_UpdateTransaction24";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
