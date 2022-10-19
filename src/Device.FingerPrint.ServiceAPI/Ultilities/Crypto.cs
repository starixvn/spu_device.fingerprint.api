using System.Security.Cryptography;
using System.Text;

namespace Device.FingerPrint.ServiceAPI.Ultilities
{
    public class Crypto
    {
        public string GetMD5Hash(string str)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytes = ASCIIEncoding.Default.GetBytes(str);
            byte[] encoded = md5.ComputeHash(bytes);

            var sb = new StringBuilder();
            for (int i = 0; i < encoded.Length; i++)
                sb.Append(encoded[i].ToString("x2"));

            return sb.ToString();
        }
    }
}
