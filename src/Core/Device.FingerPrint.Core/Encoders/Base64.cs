namespace Device.FingerPrint.Core.Encoders
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class Base64
    {
        public static string Decode(string encodedData)
        {
            var validEncodedData = ValidEncodedData(encodedData);

            byte[] bytes = Convert.FromBase64String(validEncodedData.Replace(" ", "+"));

            return Encoding.UTF8.GetString(bytes);
        }

        private static string ValidEncodedData(string encodedData)
        {
            string pattern = "^[A-Za-z0-9+/]*={0,2}$";

            Match match = Regex.Match(encodedData, pattern);

            if (!match.Success)
            {
                int indexOfFirstInvalidChar = encodedData.IndexOf(encodedData.First(c => !Regex.IsMatch(c.ToString(), "[A-Za-z0-9+/=]")));

                encodedData = encodedData.Substring(0, indexOfFirstInvalidChar);
            }

            return encodedData;
        }
    }
}
