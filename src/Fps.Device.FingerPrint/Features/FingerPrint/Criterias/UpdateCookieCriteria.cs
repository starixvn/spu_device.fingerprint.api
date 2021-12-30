using Fanex.Data;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class UpdateCookieCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Cookie")]
        public string Code { get; set; }

        [SpParam(SqlParamName = "op_Id")]
        public long Id { get; set; }

        public override string GetSettingKey()
        {
            return "FP_UpdateCookie";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
