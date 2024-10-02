using Fanex.Data;
using Fanex.Data.Repository;
using System;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class GetAssociationBySubscriberDeviceCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_SubscriberID")]
        public int SubscriberId { get; set; }

        [SpParam(SqlParamName = "ip_FirstDeviceCode")]
        public string DeviceCode { get; set; }

        [SpParam(SqlParamName = "ip_FromDate")]
        public DateTime FromDate { get; set; }

        [SpParam(SqlParamName = "ip_ToDate")]
        public DateTime ToDate { get; set; }

        public override string GetSettingKey()
        {
            return "DCS_DC_Association_GetAssociationByDevice";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
