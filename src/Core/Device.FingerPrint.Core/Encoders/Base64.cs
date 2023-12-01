namespace Device.FingerPrint.Core.Encoders
{
    using System;
    using System.Text;

    public class Base64
    {
        public static string Decode(string encodedData)
        {
            byte[] bytes = Convert.FromBase64String(encodedData.Replace(" ", "+"));
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
