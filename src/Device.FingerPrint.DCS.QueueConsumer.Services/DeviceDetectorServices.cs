using Device.FingerPrint.DCS.QueueConsumer.Services.Encoders;
using Device.FingerPrint.DCS.QueueConsumer.Services.Model;
using Newtonsoft.Json;

namespace Device.FingerPrint.DCS.QueueConsumer.Services
{
    public interface IDeviceDetectorServices
    {
        DeviceInfo GetDeviceInfo(string deviceCode);
    }

    public class DeviceDetectorServices : IDeviceDetectorServices
    {
        public DeviceInfo GetDeviceInfo(string deviceInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceInfo))
                {
                    return GenerateEmptyDeviceInfo();
                }

                var decryptedText = Base64.Decode(deviceInfo);
                return JsonConvert.DeserializeObject<DeviceInfo>(decryptedText);
            }
            catch
            {
                return GenerateEmptyDeviceInfo();
            }
        }

        private static DeviceInfo GenerateEmptyDeviceInfo()
        {
            return new DeviceInfo
            {
                DeviceCode = string.Empty,
                AppVersion = string.Empty,
                TimeZone = string.Empty,
                UserAgent = string.Empty,
                DeviceId = "N/A",
                Screen = new DeviceScreen(),
                HRef = string.Empty
            };
        }
    }
}
