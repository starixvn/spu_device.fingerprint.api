using Fanex.Data;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class MergeTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_Size")]
        public int Size { get; set; }


        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_Transaction_Merge";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
