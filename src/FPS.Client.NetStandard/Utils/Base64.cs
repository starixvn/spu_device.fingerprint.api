using System;
using System.Collections.Generic;
using System.Text;

namespace FPS.Client.NetStandard.Utils
{
    internal class Base64
    {
        public static string Decode(string encodedData)
        {
            var encodedDataAsBytes = Convert.FromBase64String(encodedData.Replace(" ", "+"));
            return Encoding.UTF8.GetString(encodedDataAsBytes);
        }
    }
}
