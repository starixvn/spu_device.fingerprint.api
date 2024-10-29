using System;

namespace Device.FingerPrint.PortalAPI.Models
{
    public class WhoAmIModel
    {
        public string ServerName { get; set; } 

        public string LocalIP { get; set; }

        public DateTime  ServerTime { get; set; }

        public string RealIP { get; set; }

        public string FakeIP { get; set; }

        public string XFW { get; set; }

        public string ClientIP { get; set; }

        public string ActualIP { get; set; }

    }
}
