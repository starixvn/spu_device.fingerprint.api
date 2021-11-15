using Fanex.Data;
using Fanex.Data.Repository;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class InsertFingerPrintCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_DeviceIdentify")]
        public string DeviceIdentify { get; set; }

        [SpParam(SqlParamName = "ip_RoutingKey")]
        public string RoutingKey { get; set; }

        [SpParam(SqlParamName = "ip_Attributes")]
        public string Attributes { get; set; }

        public override string GetSettingKey()
        {
            return "FP_InsertFingerPrint";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
