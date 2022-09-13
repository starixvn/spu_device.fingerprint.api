using Device.Fingerprint.WebAPI.Models;
using Device.Fingerprint.WebAPI.Ultilities;
using DeviceDetectorNET;

namespace Device.Fingerprint.WebAPI.Services
{
    public class TransactionAttributeService
    {

        public string GenerateFPAttributesCode(string fingerPrintCode, FingerPrintAttributeModel model, Crypto crypto)
        {
            var concatString = fingerPrintCode + "_doNotTrack=" + model.DoNotTrack + "_glRenderer=" + model.GLRenderer + "_accept=" + model.HeaderAccept + "_encoding=" + model.HeaderAcceptEncoding + "_headers=" + model.Headers;
            var fpAttributeCode = crypto.GetMD5Hash(concatString);
            return fpAttributeCode;
        }

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
