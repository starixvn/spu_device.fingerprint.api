using System;

namespace Device.FingerPrint.PortalAPI.Models
{
    public class WhoAmIModel
    {
        public string ServerName { get; set; } 

        public string LocalIP { get; set; }

        public DateTime  ServerTime { get; set; }

    }
}
