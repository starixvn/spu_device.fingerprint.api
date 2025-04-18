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

    public class EcService(
        IAppSettings appSettings,
        IHttpContextAccessor contextAccessor,
        HttpCachingWrapper cacheWrapper,
        IRecoverDeviceService recoverDeviceService) : IEcService
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
                await WriteEmptyContentAsync();
                return;
            }

            var requestIp = GetClientIp();
            var deviceDetection = ParseDeviceInfo(requestIp);
            var candidateDeviceIds = GetCandidateDeviceIds(deviceDetection);
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
                SyncServerStorage(recoveredDevice.DeviceId);
                _cacheWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);

                await WriteDeviceIdResponseAsync(recoveredDevice.DeviceId);
            }
        }

        #region Device Info Helpers

        private string GetClientIp()
        {
            var ipParam = Request?.Query["ip"].ToString();
            return string.IsNullOrEmpty(ipParam)
                ? Utilities.GetUserHostAddress()
                : ClientEncoder.Decrypt(Uri.UnescapeDataString(ipParam));
        }

        private DeviceDetection? ParseDeviceInfo(string ip)
        {
            var encrypted = Base64.Decode(Uri.UnescapeDataString(Request?.Query["deviceInfo"] ?? string.Empty));
            var decrypted = Rc4.Decrypt(ip, encrypted);

            return JsonConvert.DeserializeObject<DeviceDetection>(decrypted);
        }

        private List<string>? GetCandidateDeviceIds(DeviceDetection? detection)
        {
            var clientIds = detection?.Device?.ClientDeviceIds;
            var cookieId = GetDeviceIdFromCookie();

            var candidates = clientIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(DeviceFactory.IsValidDeviceId)
                .Select(DeviceEncoder.Decrypt)
                .Distinct()
                .ToList() ?? [];

            if (!string.IsNullOrEmpty(cookieId) && !candidates.Contains(cookieId))
            {
                candidates.Add(cookieId);
            }

            return candidates;
        }

        private string GetDeviceIdFromCookie()
        {
            var token = Request?.Cookies[_appSettings.CookieStorage];

            return DeviceFactory.IsValidDeviceId(token)
                ? DeviceEncoder.Decrypt(token)
                : string.Empty;
        }
        #endregion

        #region Insert / Sync Device

        private void TryInsertNewDevice(RecoveredDevice? recovered, DeviceDetection? detection)
        {
            if (recovered?.IsNewDeviceId == true &&
                detection?.Device?.Code is not null)
            {
                detection.Device.Id = DeviceEncoder.Decrypt(recovered.DeviceId);
                InsertNewDevice(detection);
            }
        }

        private void InsertNewDevice(DeviceDetection detection)
        {
            var criteria = new InsertDeviceCriteria
            {
                DeviceId = detection.Device.Id,
                DeviceCode = detection.Device.Code,
                Navigator = detection.Device.Navigator,
                Plugins = detection.Device.Plugins,
                MimeTypes = detection.Device.MimeTypes,
                UserAgent = detection.Device.UserAgent,
                Screen = detection.Device.Screen,
                Monitor = detection.Device.Monitor,
                Language = detection.Device.Language,
                CookieEnabled = detection.Device.CookieEnabled,
                DoNotTrack = detection.Device.DoNotTrack,
                SuperCookies = detection.Device.SuperCookies,
                GpuCode = detection.Device.GPUCode,
                WebGL = detection.Device.WebGL,
                Fonts = detection.Device.Fonts,
                TimeZone = detection.Device.TimeZone,
                SystemTimeOffset = detection.Device.SystemTimeOffset,
                Browser = detection.Device.Browser,
                DeviceType = detection.Device.DeviceType,
                Accept = detection.Device.Accept,
                Headers = detection.Device.Headers,

                Ip = detection.Ip,
                LocalIPs = detection.LocalIPs,
                ScriptRequestedIp = detection.ScriptRequestedIp,
                SubscriberRecordedIp = detection.SubscriberRecordedIp,
                DomainName = detection.DomainName,
                DomainSameReferrer = IsSameDomain(detection.DomainName, detection.UrlRefererAuthority)
            };

            _repository.Execute(criteria);
        }

        private static bool IsSameDomain(string domain, string referer) =>
            !string.IsNullOrEmpty(domain) && domain.Equals(referer, StringComparison.OrdinalIgnoreCase);

        private void SyncServerStorage(string deviceId)
        {
            _cacheWrapper.ETag = deviceId;
            SetCookie(_appSettings.CookieStorage, deviceId);
        }

        private void SetCookie(string key, string value)
        {
            var options = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddYears(10),
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                Secure = true,
                Path = "/"
            };

            Response?.Cookies.Append(key, value, options);
        }

        #endregion

        #region Response Output

        private async Task WriteEmptyContentAsync()
        {
            await WriteJavascriptContentAsync(string.Empty);
        }

        private async Task WriteDeviceIdResponseAsync(string deviceId)
        {
            var jsContent = $"try{{Detecas.Core.callback('{deviceId}')}}catch(e){{}}";
            await WriteJavascriptContentAsync(jsContent);
        }

        private async Task WriteJavascriptContentAsync(string content)
        {
            Response!.ContentType = "text/javascript";

            string minified = MinifyJavaScript(content);
            await Response.WriteAsync(minified + "\n");
        }

        private static string MinifyJavaScript(string js)
        {
            var result = Uglify.Js(js);

            if (result.HasErrors)
            {
                throw new InvalidOperationException("JavaScript minification failed.");
            }

            return result.Code;
        }

        #endregion
    }
}
