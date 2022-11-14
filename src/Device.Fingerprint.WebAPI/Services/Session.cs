using Device.Fingerprint.WebAPI.Ultilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Fingerprint.WebAPI.Services
{
    public static class Session
    {
        public static string Generate()
        {
            var guid = Guid.NewGuid().ToString("N");
            var expiredTime = DateTime.UtcNow.AddMinutes(1);

            return string.Format("{0}{1}{2}{3}{4}"
                , Hashing.GetRandomHexNumber(8).ToLower()
                , expiredTime.ToString("yyyyMMdd")
                , guid
                , expiredTime.ToString("hhmmss")
                , Hashing.GetRandomHexNumber(8).ToLower());
        }

        public static string Generate(int subscriberId)
        {
            var guid = Guid.NewGuid().ToString("N");

            return string.Format("{0}-{1}", subscriberId, guid);
        }
    }
}
