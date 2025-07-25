using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Device.FingerPrint.Features.Transaction.Model
{
    public class TransactionByLoginNameParamModel
    {
        public int SubscriberID { get; set; }

        public string LoginName { get; set; }

        public int BatchSize { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
