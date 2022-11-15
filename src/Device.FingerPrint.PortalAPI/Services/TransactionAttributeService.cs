using Device.FingerPrint.PortalAPI.Models;
using DeviceDetectorNET;

namespace Device.FingerPrint.PortalAPI.Services
{
    public class TransactionAttributeService
    {
        public UserAgentInfoModel GetUserAgentInfo(string userAgent)
        {
            var deviceDetector = new DeviceDetector(userAgent);

            deviceDetector.SkipBotDetection();
            deviceDetector.Parse();

            var userAgentInfo = new UserAgentInfoModel();

            var osInfo = deviceDetector.GetOs();

            if (osInfo != null && osInfo.Success)
            {
                userAgentInfo.OSName = osInfo.Match.Name;
            }

            var clientInfo = deviceDetector.GetClient();

            if (clientInfo != null && clientInfo.Success)
            {
                userAgentInfo.BrowserName = clientInfo.Match.Name;
                userAgentInfo.BrowserVersion = clientInfo.Match.Version;
            }

            return userAgentInfo;
        }
    }
}
