using Fanex.Data;
using Fanex.Data.Repository;
using System;
using System.Security.Permissions;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class GetAssociationByDateRangeSubscriberCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_SubscriberID")]
        public int SubscriberId { get; set; }

        [SpParam(SqlParamName = "ip_FromDate")]
        public DateTime FromDate { get; set; }

        [SpParam(SqlParamName = "ip_Limit")]
        public int Limit { get; set; }
       

        public override string GetSettingKey()
        {
            return "DCS_DC_Association_GetAssociationByDateRangeSubscriber";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
