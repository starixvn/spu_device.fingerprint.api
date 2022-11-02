using System;
using System.Text;

namespace Device.FingerPrint.QueueConsumerDCS.Services.Encoders
{
    public static class Base64
    {
        public static string Decode(string encodedData)
        {
            var encodedDataAsBytes = Convert.FromBase64String(encodedData.Replace(" ", "+"));
            return Encoding.UTF8.GetString(encodedDataAsBytes);
        }
    }
}
