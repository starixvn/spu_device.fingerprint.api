using Device.FingerPrint.Core.Services;
using Newtonsoft.Json;

namespace Device.FingerPrint.QueueConsumer
{
    public class Utilities
    {
        public static string GetMD5HashFromObject(ICrypto crypto, object? value)
        {
            return crypto.GetMD5Hash(JsonConvert.SerializeObject(value));
        }
    }
}
