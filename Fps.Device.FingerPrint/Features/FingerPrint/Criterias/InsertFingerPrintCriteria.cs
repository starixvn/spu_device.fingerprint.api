using Fanex.Data.Repository;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class InsertFingerPrintCriteria : NonQueryCommand
    {
        public string  DeviceIndentify { get; set; }

        public long AttributeId { get; set; }

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
