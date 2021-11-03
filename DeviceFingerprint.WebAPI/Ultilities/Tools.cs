using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Ultilities
{
    public static class Tools
    {
        public static string HexaString(int byteNumber)
        {
            var csprng = new RNGCryptoServiceProvider();
            var bytes = new byte[byteNumber];

            csprng.GetNonZeroBytes(bytes);

            return string.Join("", bytes.Select(b => b.ToString("x2")));
        }

        public static string GenerateRandomKey(int subscriberId)
        {
            var key = new StringBuilder();
            var rand = new Random();

            key.Append(subscriberId);
            key.Append(DateTimeOffset.Now.ToUnixTimeMilliseconds());
            key.Append(rand.Next(1000, 999999));

            return key.ToString();
        }
    }
}
