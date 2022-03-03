using Fanex.Data;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.Transaction.Criterias
{
    public class UpdateMergeTransactionCriteria : NonQueryCommand
    {
        [SpParam(SqlParamName = "ip_TransJson")]
        public string Transaction { get; set; }


        public override string GetSettingKey()
        {
            return "FPS_FP_Transform_MergeTransaction_Update";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
