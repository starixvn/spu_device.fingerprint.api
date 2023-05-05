using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSAKeyGenerator
{
    internal class PemBuilder
    {
        private readonly byte[] _bytes;
        private readonly string _type;

        public PemBuilder(byte[] bytes, string type)
        {
            _bytes = bytes;
            _type = type;
        }

        public string Build()
        {
            var base64 = Convert.ToBase64String(_bytes);

            using var stringWriter = new StringWriter();
            stringWriter.WriteLine($"-----BEGIN {_type}-----");

            const int LineLength = 64;
            for (int i = 0; i < base64.Length; i += LineLength)
            {
                stringWriter.WriteLine(base64.Substring(i, Math.Min(LineLength, base64.Length - i)));
            }

            stringWriter.WriteLine($"-----END {_type}-----");

            return stringWriter.ToString();
        }
    }
}

