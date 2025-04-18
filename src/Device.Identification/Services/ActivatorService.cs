using Device.Identification.ActivatorScript;
using Device.Identification.Factory;
using Microsoft.Net.Http.Headers;

namespace Device.Identification.Services
{
    public interface IActivatorService
    {
        Task Execute();
    }

    public class ActivatorService(
        IAppSettings appSettings,
        IHttpContextAccessor contextAccessor,
        ActivatorScriptFactory scriptFactory,
        HttpCachingWrapper httpCachingWrapper) : IActivatorService
    {
        private readonly IAppSettings _appSettings = appSettings;
        private readonly ActivatorScriptFactory _scriptFactory = scriptFactory;
        private readonly HttpCachingWrapper _httpCachingWrapper = httpCachingWrapper;

        private HttpRequest? Request => contextAccessor.HttpContext?.Request;
        private HttpResponse? Response => contextAccessor.HttpContext?.Response;

        public async Task Execute()
        {
            if (_appSettings.IsDisabledDI || Request == null || Response == null)
            {
                return;
            }

            var fdiCookieKey = "gfp";
            var cachedDeviceId = GetCachedDeviceId();

            var defaultDeviceId = string.IsNullOrEmpty(cachedDeviceId)
                                    ? DeviceFactory.GenerateDeviceId()
                                    : cachedDeviceId;

            SetETagIfValid(defaultDeviceId);

            var fdiKey = GetCookieValue(fdiCookieKey);
            _httpCachingWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);

            await WriteActivatorContentAsync(cachedDeviceId, defaultDeviceId, fdiKey);
        }

        private string GetCachedDeviceId()
        {
            var id = GetCookieValue(_appSettings.CookieStorage, "(global.c3)");
            return string.IsNullOrEmpty(id) ? GetDeviceIdFromETag() : id;
        }

        private string GetCookieValue(string key, string headerKey = "")
        {
            var value = GetCookieFromRequest(key) ?? GetCookieFromHeaders(headerKey);
            return DeviceFactory.IsValidDeviceId(value) ? value : string.Empty;
        }

        private void SetETagIfValid(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId)) return;

            _httpCachingWrapper.ETag = deviceId;

            if (!Response!.HasStarted)
            {
                Response.Headers[HeaderNames.ETag] = deviceId;
            }
        }

        private async Task WriteActivatorContentAsync(string cachedDeviceId, string defaultDeviceId, string fdiKey)
        {
            var acceptHeader = Request?.Headers.Accept.ToString();
            var headers = string.Join(",", GetAcceptEncodingHeaders());

            var host = Request?.Host.Host;
            var port = Request?.Host.Port?.ToString();
            var domain = host == "localhost" ? $"{host}:{port}" : host ?? "unknown";

            var scriptBuilder = _scriptFactory.CreateConfigScript(
                cachedDeviceId,
                defaultDeviceId,
                acceptHeader,
                headers,
                domain,
                fdiKey);

            var subscriberScript = scriptBuilder.CreateConfigScript();
            await subscriberScript.WriteScript(Response!);
        }

        private List<string> GetAcceptEncodingHeaders()
        {
            return Request?.Headers.AcceptEncoding
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .ToList() ?? [];
        }

        private string GetDeviceIdFromETag()
        {
            var deviceId = _httpCachingWrapper.ETag;
            return DeviceFactory.IsValidDeviceId(deviceId) ? deviceId : string.Empty;
        }

        private string? GetCookieFromRequest(string key)
        {
            return Request?.Cookies.TryGetValue(key, out var value) == true ? value : null;
        }

        private string GetCookieFromHeaders(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || Request?.Headers.Cookie.ToString() is not string rawCookies || string.IsNullOrEmpty(rawCookies))
                return string.Empty;

            var cookies = rawCookies.Split(';')
                .Select(c => c.Split('=', 2))
                .ToDictionary(
                    parts => parts[0].Trim(),
                    parts => parts.Length > 1 ? parts[1].Trim() : string.Empty);

            return cookies.TryGetValue(key, out var value) ? value : string.Empty;
        }
    }
}
