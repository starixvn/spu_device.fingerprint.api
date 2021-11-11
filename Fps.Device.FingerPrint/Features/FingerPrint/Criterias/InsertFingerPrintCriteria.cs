using Fanex.Data;
using Fanex.Data.Repository;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class InsertFingerPrintCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_DeviceIdentify")]
        public string  DeviceIndentify { get; set; }

        [SpParam(SqlParamName = "ip_Attributes")]
        public long AttributeId { get; set; }

        public override string GetSettingKey()
        {
            return "abc";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
