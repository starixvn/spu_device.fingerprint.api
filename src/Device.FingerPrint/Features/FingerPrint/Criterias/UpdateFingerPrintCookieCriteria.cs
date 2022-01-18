using Fanex.Data;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.FingerPrint.Criterias
{
    public class UpdateFingerPrintCookieCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_CookieId")]
        public long CookieId { get; set; }
               

        [SpParam(SqlParamName = "ip_FingerPrint")]
        public string FingerPrint { get; set; }

        public override string GetSettingKey()
        {
            return "FP_UpdateFingerPrintCookie";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
