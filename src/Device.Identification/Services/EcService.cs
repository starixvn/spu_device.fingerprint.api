using Device.Identification.Criterias;
using Device.Identification.Encoders;
using Device.Identification.Factory;
using Device.Identification.Model;
using Fanex.Data;
using Fanex.Data.Repository;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Globalization;

namespace Device.Identification.Services
{
    public interface IEcService
    {
        string GetRequestIpFromClientToParseDeviceInfo();

        DeviceDetection? ParseDeviceInfo(string requestIp);

        List<string>? GetCandidatePlainDeviceIdsFromStorages(DeviceDetection? deviceDetection);

        string RecoverDeviceIdFromCookie();

        string? GetDeviceCode(DeviceDetection? deviceDetection);

        void TryInsertNewDevice(RecoveredDevice? recoveredDevice, DeviceDetection? deviceDetection);

        void SynchronizeServerStorages(RecoveredDevice recoveredDevice, EverCookieCachingWrapper cacheWrapper);

        void UpdateCachingWithExpiredDaysAndModifiedTime(int days);
    }

    public class EcService(IAppSettings appSettings) : IEcService
    {
        private static readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();
        private readonly IAppSettings _appSettings = appSettings;
        private readonly IDynamicRepository _repository = new DynamicRepository();

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

        public void TryInsertNewDevice(RecoveredDevice? recoveredDevice, DeviceDetection? deviceDetection)
        {
            if (recoveredDevice != null && deviceDetection != null
                    && recoveredDevice.IsNewDeviceId
                    && !string.IsNullOrEmpty(deviceDetection.Device?.Code)
            )
            {
                deviceDetection.Device.Id = DeviceEncoder.Decrypt(recoveredDevice.DeviceId);
                InsertNewDeviceAsync(deviceDetection);
            }
            //else
            //{
            //    if (!string.IsNullOrEmpty(deviceDetection.LocalIPs))
            //    {
            //        TryUpdateLocalIps();
            //    }
            //}
        }

        private void InsertNewDeviceAsync(DeviceDetection deviceDetection)
        {
            var criteria = new InsertDeviceCriteria
            {
                DeviceId = deviceDetection.Device.Id,
                DeviceCode = deviceDetection.Device.Code,
                Navigator = deviceDetection.Device.Navigator,
                Plugins = deviceDetection.Device.Plugins,
                MimeTypes = deviceDetection.Device.MimeTypes,
                UserAgent = deviceDetection.Device.UserAgent,
                Screen = deviceDetection.Device.Screen,
                Monitor = deviceDetection.Device.Monitor,
                Language = deviceDetection.Device.Language,
                CookieEnabled = deviceDetection.Device.CookieEnabled,
                DoNotTrack = deviceDetection.Device.DoNotTrack,
                SuperCookies = deviceDetection.Device.SuperCookies,
                GpuCode = deviceDetection.Device.GPUCode,
                WebGL = deviceDetection.Device.WebGL,
                Fonts = deviceDetection.Device.Fonts,
                TimeZone = deviceDetection.Device.TimeZone,
                SystemTimeOffset = deviceDetection.Device.SystemTimeOffset,
                Browser = deviceDetection.Device.Browser,
                DeviceType = deviceDetection.Device.DeviceType,
                Accept = deviceDetection.Device.Accept,
                Headers = deviceDetection.Device.Headers,
                //CreatedDate = DateTime.Now,

                Ip = deviceDetection.Ip,
                LocalIPs = deviceDetection.LocalIPs,
                RemoteIPs = deviceDetection.RemoteIPs,
                ScriptRequestedIp = deviceDetection.ScriptRequestedIp,
                SubscriberRecordedIp = deviceDetection.SubscriberRecordedIp,
                DomainName = deviceDetection.DomainName,
                DomainSameReferrer = IsDomainSameWithReferrer(deviceDetection.DomainName, deviceDetection.UrlRefererAuthority)
            };

            _repository.Execute(criteria);
        }

        private static bool IsDomainSameWithReferrer(string domainName, string urlRefererAuthority)
        {
            return !string.IsNullOrEmpty(domainName) && domainName.Equals(urlRefererAuthority);
        }

        private void TryUpdateLocalIps(RecoveredDevice recoveredDevice, DeviceDetection deviceDetection)
        {
            var emptyLocalIps = "[]";
            if (deviceDetection.LocalIPs != emptyLocalIps)
            {
                var criteria = new FingerprintLocalIpUpdateCriteria
                {
                    DeviceId = DeviceEncoder.Decrypt(recoveredDevice.DeviceId),
                    Ip = deviceDetection.Ip,
                    LocalIps = deviceDetection.LocalIPs
                };

                //UpdateLocalIps();
                //if (!string.IsNullOrEmpty(criteria.Message))
                //{
                //    LogService.WriteError("TryUpdateLocalIps function returns " +
                //        criteria.Message +
                //        "\r\nParameters: " +
                //        JsonConvert.SerializeObject(criteria));
                //}
            }
        }

        public void UpdateLocalIps()
        {
            var criteria = new FingerprintLocalIpUpdateCriteria();

            using (var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey()))
            {
                var result = db.ExecuteNonQuery(criteria);
                var errorModel = new ErrorModel
                {
                    Error = criteria.Error,
                    Message = criteria.Message
                };
            }
        }

        public void SynchronizeServerStorages(RecoveredDevice recoveredDevice, EverCookieCachingWrapper cacheWrapper)
        {
            cacheWrapper.ETag = recoveredDevice.DeviceId;
            WriteCookie(_appSettings.CookieStorage, recoveredDevice.DeviceId);
        }

        private static void WriteCookie(string key, string deviceId)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            if (response == null) return;

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddYears(10),
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                Secure = true,
                Path = "/"
            };

            response.Cookies.Append(key, deviceId, cookieOptions);
        }

        public void UpdateCachingWithExpiredDaysAndModifiedTime(int days)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            if (response == null) return;

            int maxAgeInSeconds = days * 86400;

            response.Headers[HeaderNames.CacheControl] = $"public, max-age={maxAgeInSeconds}";
            response.Headers[HeaderNames.Expires] = DateTime.UtcNow.AddDays(days)
                                                                    .ToString("R", CultureInfo.InvariantCulture);
        }

    }
}
