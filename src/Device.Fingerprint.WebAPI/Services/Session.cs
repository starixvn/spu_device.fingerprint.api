using System;

namespace Device.Fingerprint.WebAPI.Services
{
    public static class Session
    {
        public static string Generate()
        {
            var guid = Guid.NewGuid().ToString("N");

            return string.Format("{0}", guid);
        }
    }
}
