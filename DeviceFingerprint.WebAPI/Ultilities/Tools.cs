using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    }
}
