using System;
using System.Collections.Generic;
using System.Text;

namespace Fps.Device.FingerPrint.Features.Transaction.Model
{
   public class TransactionEntity
    {
        public string LoginName { get; set; }

        public int SubscriberId { get; set; }

        public string SessionKey { get; set; }

        public DateTime TransTime { get; set; }

        public string ClientIp { get; set; }

        public string UserAgent { get; set; }

    }
}
