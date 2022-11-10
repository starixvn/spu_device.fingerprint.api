using Device.FingerPrint.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.PortalAPI.Models
{
    public class CookieParameter
    {
        public ClientTransactionCookieModel ClientTransCookie { get; set; }

        public string RedisKey { get; set; }
    }
}
