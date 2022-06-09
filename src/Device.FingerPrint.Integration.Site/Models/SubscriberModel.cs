using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.Integration.Site.Models
{
    public class SubscriberModel
    {
        public int SubscriberId { get; set; }

        public string SubscriberName { get; set; }

        public int DomainId { get; set; }

        public string DomainName { get; set; }

        public bool IsActived { get; set; }

        public bool IsMain { get; set; }
    }
}
