using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Model;
using DeviceDetectorNET;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface IUserAgentService
    {
        public UserAgentInfoModel GetUserAgentInfo(string userAgent);
    }

    public class UserAgentService : IUserAgentService
    {
        private readonly ICrypto crypto;

        public UserAgentService(ICrypto crypto)
        {
            this.crypto = crypto;
        }

        public UserAgentInfoModel GetUserAgentInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return new UserAgentInfoModel();

            var deviceDetector = new DeviceDetector(userAgent);

            deviceDetector.SkipBotDetection();
            deviceDetector.Parse();

            var userAgentInfo = new UserAgentInfoModel { UserAgent = userAgent };

            var clientInfo = deviceDetector.GetClient();

            if (clientInfo != null && clientInfo.Success)
            {
                userAgentInfo.ClientType = clientInfo.Match.Type;
                userAgentInfo.ClientName = clientInfo.Match.Name;
                userAgentInfo.ClientVersion = clientInfo.Match.Version;
            }

            var osInfo = deviceDetector.GetOs();

            if (osInfo != null && osInfo.Success)
            {
                userAgentInfo.OSName = osInfo.Match.Name;
                userAgentInfo.OSShortName = osInfo.Match.ShortName;
                userAgentInfo.OSVersion = osInfo.Match.Version;
                userAgentInfo.OSPlatform = osInfo.Match.Platform;
            }

            userAgentInfo.Device = deviceDetector.GetDeviceName();
            userAgentInfo.Brand = deviceDetector.GetBrandName();
            userAgentInfo.Model = deviceDetector.GetModel();
            userAgentInfo.DeviceType = GetDeviceType(deviceDetector);

            userAgentInfo.FPUserAgentCode = crypto.GetMD5Hash(userAgent);

            return userAgentInfo;
        }

        private int GetDeviceType(DeviceDetector deviceDetector)
        {
            int _OTHERTYPE = 0;
            int _DESTOPTYPE = 1;
            int _TABLETTYPE = 2;
            int _MOBILETYPE = 3;

            if (deviceDetector.IsDesktop()) return _DESTOPTYPE;

            if (deviceDetector.IsTablet()) return _TABLETTYPE;

            if (deviceDetector.IsMobile()) return _MOBILETYPE;

            return _OTHERTYPE;
        }
    }
}
