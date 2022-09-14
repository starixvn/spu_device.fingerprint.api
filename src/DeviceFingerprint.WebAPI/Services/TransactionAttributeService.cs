using Device.Fingerprint.WebAPI.Models;
using Device.Fingerprint.WebAPI.Ultilities;
using DeviceDetectorNET;

namespace Device.Fingerprint.WebAPI.Services
{
    public class TransactionAttributeService
    {
        public string GenerateFPAttributesCode(FingerPrintMoreAttributeModel fpModel, HeaderModel headerModel, Crypto crypto)
        {
            var doNotTrack = string.IsNullOrEmpty(fpModel.DoNotTrack) ? "undefined" : fpModel.DoNotTrack;
            var glRenderer = string.IsNullOrEmpty(fpModel.GLRenderer) ? "undefined" : fpModel.GLRenderer;

            //var concatString = fpModel.FingerPrintCode + "_#_doNotTrack=" + doNotTrack + "_#_glRenderer=" + glRenderer + "_#_accept=" + headerModel.Accept + "_#_encoding=" + headerModel.Encoding + "_#_headers=" + headerModel.Keys;
            var concatString = fpModel.FingerPrintCode + "_#_doNotTrack=" + doNotTrack + "_#_glRenderer=" + glRenderer;
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
