namespace Device.Identification.Factory
{
    using System;
    using Newtonsoft.Json;
    using Device.Identification.Encoders;
    using Device.Identification.Model;
    using Microsoft.Extensions.Logging;

    public static class DeviceFactory
    {
        public static string GenerateDeviceId()
        {
            var guid = Guid.NewGuid();
            var id = guid.ToString("N");

            return DeviceEncoder.Encrypt(id);
        }

        public static bool IsValidDeviceId(string deviceId)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceId))
                {
                    return false;
                }

                deviceId = DeviceEncoder.Decrypt(deviceId);

                return Guid.TryParse(deviceId, out _);
            }
            catch (Exception exception)
            {
                if (!deviceId.Contains("TURBO"))
                {
                    //_logger.LogError(exception, "DeviceId {DeviceId} is invalid", deviceId);
                }
            }

            return false;
        }

        public static DeviceInfo GetDeviceInfo(string deviceInfo)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceInfo))
                {
                    return GenerateEmptyDeviceInfo();
                }

                var decryptedText = Base64.Decode(deviceInfo);
                return JsonConvert.DeserializeObject<DeviceInfo>(decryptedText, Identification.Configuration.JsonSerializerSettingsFactory.Instance);
            }
            catch (Exception exception)
            {
                //_logger.LogError(exception, "DeviceInfo {DeviceInfo} is invalid", deviceInfo);

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
                DeviceId = Configurations.EmptyValue,
                Screen = new DeviceScreen(),
                HRef = string.Empty,
                BotD = new BotD()
            };
        }
    }
}
