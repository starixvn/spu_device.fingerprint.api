using Azure.Core;
using Device.Identification.ActivatorScript;
using Device.Identification.Factory;
using Microsoft.Net.Http.Headers;
using System.Security.AccessControl;

namespace Device.Identification.Services
{
    public interface IActivatorService
    {
        Task Execute();
    }

    public class ActivatorService(IAppSettings appSettings
                                , IHttpContextAccessor contextAccessor
                                , ActivatorScriptFactory scriptFactory
                                , HttpCachingWrapper httpCachingWrapper) : IActivatorService
    {
        private readonly IAppSettings _appSettings = appSettings;
        private readonly HttpCachingWrapper _httpCachingWrapper = httpCachingWrapper;
        private readonly ActivatorScriptFactory _scriptFactory = scriptFactory;

        private HttpRequest? Request => contextAccessor.HttpContext?.Request;
        private HttpResponse? Response => contextAccessor.HttpContext?.Response;

        public async Task Execute()
        {
            if (_appSettings.IsDisabledDI || Request == null || Response == null)
            {
                return;
            }

            var fdiCookieKey = "gfp";
            var cachedDeviceId = GetCachedCandidateDeviceId();
            var defaultDeviceId = cachedDeviceId;
            if (string.IsNullOrEmpty(defaultDeviceId))
            {
                defaultDeviceId = DeviceFactory.GenerateDeviceId();
            }

            SetEtagIfDeviceIdIsValid(defaultDeviceId);

            var fdiKey = GetCookieValue(fdiCookieKey);
            _httpCachingWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);
            await WriteActivatorContent(cachedDeviceId, defaultDeviceId, fdiKey);
        }

        private string GetCachedCandidateDeviceId()
        {
            var cachedDeviceId = GetCookieValue(_appSettings.CookieStorage, headerKey: "(global.c3)");
            if (string.IsNullOrEmpty(cachedDeviceId))
            {
                cachedDeviceId = GetDeviceIdFromETag();
            }

            return cachedDeviceId;
        }

        private string GetCookieValue(string key, string headerKey = "")
        {
            string cookieValue = GetCookieFromRequestCookies(key) ?? GetCookieFromHeaders(headerKey);

            if (!string.IsNullOrEmpty(cookieValue) && DeviceFactory.IsValidDeviceId(cookieValue))
            {
                return cookieValue;
            }

            return string.Empty;
        }

        private void SetEtagIfDeviceIdIsValid(string cachedDeviceId)
        {
            if (!string.IsNullOrEmpty(cachedDeviceId))
            {
                _httpCachingWrapper.ETag = cachedDeviceId;

                if (!Response.HasStarted)
                {
                    Response.Headers[HeaderNames.ETag] = cachedDeviceId;
                }
            }
        }

        private async Task WriteActivatorContent(string cachedDeviceId, string defaultDeviceId, string fdiKey)
        {
            var accept = Request?.Headers.Accept.ToString();
            var headers = string.Join(",", GetHeaders());
            var domainName = Request?.Host.Host == "localhost" ? Request.Host.Host + ":" + Request.Host.Port : Request?.Host.Host;

            var scriptFactory = _scriptFactory.CreateConfigScript(
                cachedDeviceId,
                defaultDeviceId,
                accept,
                headers,
                domainName,
                fdiKey);

            var subscriberScript = scriptFactory.CreateConfigScript();

            await subscriberScript.WriteScript(Response);
        }

        private List<string> GetHeaders()
        {
            return [.. Request.Headers.AcceptEncoding.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)];
        }

        private string GetDeviceIdFromETag()
        {
            var deviceId = _httpCachingWrapper.ETag;

            if (!string.IsNullOrEmpty(deviceId) && DeviceFactory.IsValidDeviceId(deviceId))
            {
                return deviceId;
            }

            return string.Empty;
        }

        private string? GetCookieFromRequestCookies(string key)
        {
            if (Request.Cookies.TryGetValue(key, out var value))
            {
                return value;
            }

            return null;
        }

        private string GetCookieFromHeaders(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            var rawCookies = Request.Headers.Cookie.ToString();
            if (string.IsNullOrEmpty(rawCookies))
            {
                return string.Empty;
            }

            var cookies = rawCookies.Split(';')
                .Select(cookie => cookie.Split(['='], 2))
                .ToDictionary(
                    parts => parts[0].Trim(),
                    parts => parts.Length > 1 ? parts[1].Trim() : string.Empty
                );

            if (cookies.TryGetValue(key, out var headerValue))
            {
                return headerValue;
            }

            return string.Empty;
        }
    }
}
