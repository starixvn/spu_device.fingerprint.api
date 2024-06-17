namespace FPS.Client.NetStandard
{
    using System;
    using System.Collections.Generic;

    public static class Token
    {
        private const string HexDigit = "0123456789abcdef";

        private const string EncryptKey = "&f％p#s@m?b,c:*!~^._";

        public static string Generate()
        {
            string id = DateTime.Now.ToUniversalTime().ToString("yyyy/MM/dd HH:mm:ss") + "@" + new Random().Next(0, 255).ToString();
            id = Encrypt(id);

            return id;
        }

        private static string Encrypt(string plaintext)
        {
            var encryptedText = string.Empty;
            encryptedText = RC4(EncryptKey, plaintext);
            return EncodeToHex(encryptedText);
        }

        private static string EncodeToHex(string data)
        {
            var b16_digits = HexDigit;
            string[] b16_map = new string[256];
            for (var i = 0; i < 256; i++)
            {
                b16_map[i] = string.Concat(b16_digits[i >> 4], b16_digits[i & 15]);
            }

            int c = data.Length;
            List<string> result = new List<string>();

            for (var i = 0; i < c; i++)
            {
                result.Add(b16_map[Convert.ToInt32(data[i])]);
            }

            return string.Join(null, result.ToArray());
        }

        private static string RC4(string key, string value)
        {
            char[] ks = key.ToCharArray();
            char[] vs = value.ToCharArray();
            int c = key.Length;
            int d = value.Length;
            int[] s = new int[256];
            int i = 0;
            for (i = 0; i < 256; i++)
            {
                s[i] = i;
            }

            int j = 0;
            int x;
            for (i = 0; i < 256; i++)
            {
                j = (j + s[i] + (int)ks[i % c]) % 256;
                x = s[i];
                s[i] = s[j];
                s[j] = x;
            }

            i = 0;
            j = 0;
            string ct = string.Empty;
            for (int y = 0; y < d; y++)
            {
                i = (i + 1) % 256;
                j = (j + s[i]) % 256;
                x = s[i];
                s[i] = s[j];
                s[j] = x;
                ct += Convert.ToChar((int)vs[y] ^ s[(s[i] + s[j]) % 256]);
            }

            return ct;
        }
    }
}
