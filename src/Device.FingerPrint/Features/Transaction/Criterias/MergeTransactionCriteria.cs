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
            return "FP_DeviceIdentify";
        }

        public override bool IsValid()
        {
            return true;
        }
    }
}
