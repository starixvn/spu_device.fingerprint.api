using Fanex.Data.Repository;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class InsertAtrributeCriteria : NonQueryCommand
    {
        public override string GetSettingKey()
        {
            return "";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
