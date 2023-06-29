using System;

namespace Device.Fingerprint.WebAPI.Models
{
    public class WhoamiModel
    {
        public string ServerName { get; set; }

        public string ServerIP { get; set; }

        public int Port { get; set; }

        public DateTime ServerTime { get; set; }

        public string ClientIP { get; set; }

        public string HTTP_X_FORWARDED_FOR { get; set; }

        public string X_FORWARDED_FOR { get; set; }

        public string PROXY_CLIENT_IP { get; set; }

        public string WL_PROXY_CLIENT_IP { get; set; }

        public string HTTP_CLIENT_IP { get; set; }

        public string REMOTE_ADDR { get; set; }

    }
}
