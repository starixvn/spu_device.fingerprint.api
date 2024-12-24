using Device.Identification.Encoders;
using Device.Identification.Factory;
using Device.Identification.Model;
using Newtonsoft.Json;

namespace Device.Identification.Services
{
    public interface IEcService
    {
        string GetRequestIpFromClientToParseDeviceInfo();

        DeviceDetection? ParseDeviceInfo(string requestIp);

        List<string>? GetCandidatePlainDeviceIdsFromStorages(DeviceDetection? deviceDetection);

        string RecoverDeviceIdFromCookie();

        string? GetDeviceCode(DeviceDetection? deviceDetection);
    }

    public class EcService(IAppSettings appSettings) : IEcService
    {
        private static readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private readonly IAppSettings _appSettings = appSettings;

        public string GetRequestIpFromClientToParseDeviceInfo()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;

            string requestIp = request.Query["ip"].ToString();
            return string.IsNullOrEmpty(requestIp)
                ? Utilities.GetUserHostAddress()
                : ClientEncoder.Decrypt(Uri.UnescapeDataString(requestIp));
        }

        public DeviceDetection? ParseDeviceInfo(string requestIp)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return null;

            var deviceInfo = request.Query["deviceInfo"];
            var decryptedDeviceInfo = Rc4.Decrypt(requestIp, Base64.Decode(Uri.UnescapeDataString(deviceInfo.ToString())));
            var deviceDetection = JsonConvert.DeserializeObject<DeviceDetection>(decryptedDeviceInfo);

            return deviceDetection;
        }
        public List<string>? GetCandidatePlainDeviceIdsFromStorages(DeviceDetection? deviceDetection)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return null;

            var clientDeviceIds = deviceDetection?.Device?.ClientDeviceIds;
            var cookieDeviceId = RecoverDeviceIdFromCookie();

            var candidateDeviceIds = clientDeviceIds?.Split(',')
                 .Where(DeviceFactory.IsValidDeviceId)
                 .Distinct()
                 .Select(DeviceEncoder.Decrypt)
                 .ToList();
            if (!string.IsNullOrEmpty(cookieDeviceId) && candidateDeviceIds != null && !candidateDeviceIds.Contains(cookieDeviceId))
            {
                candidateDeviceIds.Add(cookieDeviceId);
            }

            return candidateDeviceIds;
        }

        public string RecoverDeviceIdFromCookie()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return string.Empty;

            var tokenCookie = request.Cookies[_appSettings.CookieStorage];

            if (tokenCookie != null)
            {
                var cookieDeviceId = tokenCookie.ToString();
                if (DeviceFactory.IsValidDeviceId(cookieDeviceId))
                {
                    return DeviceEncoder.Decrypt(cookieDeviceId);
                }
            }

            return string.Empty;
        }

        public string? GetDeviceCode(DeviceDetection? deviceDetection)
        {
            return deviceDetection?.Device?.Code;
        }
    }
}
