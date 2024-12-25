namespace Device.Identification.Factory
{
    using Device.Identification.Encoders;
    using Device.Identification.Model;
    using Fanex.Logging;
    using Newtonsoft.Json;
    using System;

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
            catch (Exception ex)
            {
                if (!deviceId.Contains("TURBO"))
                {
                    LogService.Error(Utilities.GetLogMessage(ex));
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
            catch (Exception ex)
            {
                LogService.Error(Utilities.GetLogMessage(ex));
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
