using Fanex.Data;
using Fanex.Data.Repository;
using System;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class ValidateSuccessCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_LoginName")]
        public string LoginName { get; set; }

        [SpParam(SqlParamName = "ip_TransTime")]
        public DateTime TransTime { get; set; }

        public override string GetSettingKey()
        {
            return "DCS_DC_Transform_Transaction07_ValidateSuccess";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
