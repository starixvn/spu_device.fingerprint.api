namespace Device.Identification.Controllers
{
    using Device.Identification.ActivatorScript;
    using Device.Identification.Factory;
    using Device.Identification.Model;
    using Device.Identification.Services;
    using Microsoft.Ajax.Utilities;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;

    [ApiController]
    public class ActivatorController(IWebHostEnvironment env, IAppSettings appSettings, ScriptLoader scriptLoader, HttpCachingWrapper httpCachingWrapper, ActivatorScriptFactory scriptFactory) : ControllerBase
    {
        private readonly IWebHostEnvironment _env = env;
        private readonly IAppSettings _appSettings = appSettings;
        private readonly ScriptLoader _scriptLoader = scriptLoader;
        private readonly HttpCachingWrapper _httpCachingWrapper = httpCachingWrapper ?? throw new ArgumentNullException(nameof(httpCachingWrapper));
        private readonly ActivatorScriptFactory _scriptFactory = scriptFactory;

        private readonly string _fdiCookieKey = "gfp";

        [HttpGet("activator.ashx")]
        public async Task ActivatorAshx()
        {
            try
            {
                if (_appSettings.IsDisabledDI)
                {
                    return;
                }

                var cachedDeviceId = GetCachedCandidateDeviceId();
                var defaultDeviceId = cachedDeviceId;
                if (string.IsNullOrEmpty(defaultDeviceId))
                {
                    defaultDeviceId = DeviceFactory.GenerateDeviceId();
                    var checkValid = DeviceFactory.IsValidDeviceId(defaultDeviceId);
                }

                SetEtagIfDeviceIdIsValid(defaultDeviceId);

                var fdiKey = GetCookieValue(_fdiCookieKey);
                _httpCachingWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);
                await WriteContent(cachedDeviceId, defaultDeviceId, fdiKey);
            }
            catch (Exception ex)
            {
                var err = ex.Message;
            }
        }

        [HttpGet("tkexp.ashx")]
        public async Task TKexpAshx()
        {
            try
            {
                var subscriberScript = _scriptFactory.CreateSubscriberTokenScript();

                await subscriberScript.WriteScript(Response);
            }
            catch (Exception ex)
            {
                var err = ex.Message;
            }
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

        private string GetDeviceIdFromETag()
        {
            var deviceId = _httpCachingWrapper.ETag;

            if (!string.IsNullOrEmpty(deviceId) && DeviceFactory.IsValidDeviceId(deviceId))
            {
                return deviceId;
            }

            return string.Empty;
        }

        private void SetEtagIfDeviceIdIsValid(string cachedDeviceId)
        {
            if (!string.IsNullOrEmpty(cachedDeviceId))
            {
                _httpCachingWrapper.ETag = cachedDeviceId;
            }
        }

        private async Task WriteContent(string cachedDeviceId, string defaultDeviceId, string fdiKey)
        {
            var apiAction = Request.Query["api"].ToString() ?? string.Empty;
            var accept = Request.Headers["Accept"].ToString();
            var headers = string.Join(",", GetHeaders());
            var rawUrl = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");

            var domainName = $"{Request.Scheme}://{Request.Host}";


            var scriptFactory = _scriptFactory.CreateConfigScript(
                cachedDeviceId,
                defaultDeviceId,
                accept,
                headers,
                domainName,
                fdiKey);
            var subscriberScript = scriptFactory.CreateSubscriberScript(apiAction);

            await subscriberScript.WriteScript(Response);
        }

        private List<string> GetHeaders()
        {
            return [.. Request.Headers.AcceptEncoding.ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)];
        }
    }
}
