using Device.Identification.ActivatorScript;
using Device.Identification.Criterias;
using Device.Identification.Encoders;
using Device.Identification.Factory;
using Device.Identification.Model;
using Fanex.Data.Repository;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using NUglify;

namespace Device.Identification.Services
{
    public interface IEcService
    {
        Task Execute();
    }

    public class EcService(IAppSettings appSettings
                          , IHttpContextAccessor contextAccessor
                          , HttpCachingWrapper cacheWrapper
                          , IRecoverDeviceService recoverDeviceService) : IEcService
    {
        private readonly IAppSettings _appSettings = appSettings;
        private readonly HttpCachingWrapper _cacheWrapper = cacheWrapper;
        private readonly IRecoverDeviceService _recoverDeviceService = recoverDeviceService;
        private readonly IDynamicRepository _repository = new DynamicRepository();

        private HttpRequest? Request => contextAccessor.HttpContext?.Request;
        private HttpResponse? Response => contextAccessor.HttpContext?.Response;

        public async Task Execute()
        {
            if (string.IsNullOrEmpty(Request?.Query["deviceInfo"]) || Response == null)
            {
                await ResponseEmptyContent();
                return;
            }

            var requestIp = GetRequestIpFromClientToParseDeviceInfo();
            var deviceDetection = ParseDeviceInfo(requestIp);
            var candidateDeviceIds = GetCandidatePlainDeviceIdsFromStorages(deviceDetection);
            var currentDeviceCode = deviceDetection?.Device?.Code;

            if (_cacheWrapper.DoesRecoverDeviceIdFromETag())
            {
                _cacheWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);
                Response.StatusCode = StatusCodes.Status304NotModified;

                return;
            }

            var recoveredDevice = _recoverDeviceService.GetRecoveredDevice(candidateDeviceIds, currentDeviceCode);

            if (recoveredDevice != null)
            {
                TryInsertNewDevice(recoveredDevice, deviceDetection);
                SynchronizeServerStorages(recoveredDevice.DeviceId);
                _cacheWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);

                await ResponseInfoToClient(recoveredDevice.DeviceId);
            }
        }

        private async Task ResponseEmptyContent()
        {
            var content = string.Empty;
            await WriteContent(content);
        }

        private string GetRequestIpFromClientToParseDeviceInfo()
        {
            string requestIp = Request.Query["ip"].ToString();
            return string.IsNullOrEmpty(requestIp)
                ? Utilities.GetUserHostAddress()
                : ClientEncoder.Decrypt(Uri.UnescapeDataString(requestIp));
        }

        private DeviceDetection? ParseDeviceInfo(string requestIp)
        {
            var deviceInfo = Request.Query["deviceInfo"];
            var decryptedDeviceInfo = Rc4.Decrypt(requestIp, Base64.Decode(Uri.UnescapeDataString(deviceInfo.ToString())));
            var deviceDetection = JsonConvert.DeserializeObject<DeviceDetection>(decryptedDeviceInfo);

            return deviceDetection;
        }

        private List<string>? GetCandidatePlainDeviceIdsFromStorages(DeviceDetection? deviceDetection)
        {
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

        private string RecoverDeviceIdFromCookie()
        {
            var tokenCookie = Request.Cookies[_appSettings.CookieStorage];

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

        private void TryInsertNewDevice(RecoveredDevice? recoveredDevice, DeviceDetection? deviceDetection)
        {
            if (recoveredDevice != null && deviceDetection != null
                    && recoveredDevice.IsNewDeviceId
                    && !string.IsNullOrEmpty(deviceDetection.Device?.Code)
            )
            {
                deviceDetection.Device.Id = DeviceEncoder.Decrypt(recoveredDevice.DeviceId);
                InsertNewDeviceAsync(deviceDetection);
            }
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

                Ip = deviceDetection.Ip,
                LocalIPs = deviceDetection.LocalIPs,
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

        private void SynchronizeServerStorages(string deviceId)
        {
            _cacheWrapper.ETag = deviceId;
            AddCookie(_appSettings.CookieStorage, deviceId);
        }

        private void AddCookie(string key, string deviceId)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddYears(10),
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                Secure = true,
                Path = "/"
            };

            Response.Cookies.Append(key, deviceId, cookieOptions);
        }

        private void UpdateCachingMaxAge(int seconds)
        {
            Response.Headers[HeaderNames.CacheControl] = $"private, max-age={seconds}";
        }

        private async Task ResponseInfoToClient(string deviceId)
        {
            var content =
                "try{" +
                string.Format(
                    "Detecas.Core.callback('{0}')",
                    deviceId) +
                "}catch(e){}";

            await WriteContent(content);
        }

        private async Task WriteContent(string content)
        {
            Response.ContentType = "text/javascript";
            if (BundleConfig.EnableOptimization)
            {
                content = MinifyJavaScript(content);
            }

            await Response.WriteAsync(content + "\n");
        }

        private static string MinifyJavaScript(string content)
        {
            var result = Uglify.Js(content);

            if (result.HasErrors)
            {
                throw new InvalidOperationException("Minification failed.");
            }

            return result.Code;
        }
    }
}
