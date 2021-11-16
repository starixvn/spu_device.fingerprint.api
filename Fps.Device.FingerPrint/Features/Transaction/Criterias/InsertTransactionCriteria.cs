using Fanex.Data;
using Fanex.Data.Repository;
using System;

namespace Fps.Device.FingerPrint.Features.Transaction.Criterias
{
    public class InsertTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_LoginName")]
        public string LoginName { get; set; }

        [SpParam(SqlParamName = "ip_SubscriberId")]
        public int SubscriberId { get; set; }

        [SpParam(SqlParamName = "ip_SessionKey")]
        public string SessionKey { get; set; }

        [SpParam(SqlParamName = "ip_TransTime")]
        public DateTime TransTime { get; set; }

        [SpParam(SqlParamName = "ip_ClientIp")]
        public string ClientIp { get; set; }

        [SpParam(SqlParamName = "ip_UserAgent")]
        public string UserAgent { get; set; }


        public override string GetSettingKey()
        {
            return "FP_InsertTransaction";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
