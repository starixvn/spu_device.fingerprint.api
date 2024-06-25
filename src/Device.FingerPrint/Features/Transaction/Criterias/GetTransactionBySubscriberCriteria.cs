using Fanex.Data;
using Fanex.Data.Repository;
using System;
using System.Security.Permissions;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    class GetTransactionBySubscriberCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_SubscriberID")]
        public int SubscriberId { get; set; }

        [SpParam(SqlParamName = "ip_TransID")]
        public long TransId { get; set; }

        [SpParam(SqlParamName = "ip_Limit")]
        public int Limit { get; set; }
       

        public override string GetSettingKey()
        {
            return "DCS_DC_Transaction_GetTransactionBySubscriber";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
