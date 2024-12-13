namespace Device.Identification.Controllers
{
    using Device.Identification.ActivatorScript;
    using Device.Identification.Factory;
    using Device.Identification.Model;
    using Device.Identification.Services;
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
        public async Task<IActionResult> ActivatorAshx()
        {
            if (_appSettings.IsDisabledDI)
            {
                return BadRequest("Feature is disabled.");
            }

            var cachedDeviceId = GetCachedCandidateDeviceId();
            var defaultDeviceId = cachedDeviceId;
            if (string.IsNullOrEmpty(defaultDeviceId))
            {
                defaultDeviceId = DeviceFactory.GenerateDeviceId();
            }

            SetEtagIfDeviceIdIsValid(defaultDeviceId);

            var fdiKey = GetDeviceIdFromCookie(_fdiCookieKey);
            await WriteContent(cachedDeviceId, defaultDeviceId, fdiKey);
            _httpCachingWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);

            return Ok("Process completed successfully.");
        }

        private string GetCachedCandidateDeviceId()
        {
            var cachedDeviceId = GetDeviceIdFromCookie(_appSettings.CookieStorage);
            if (string.IsNullOrEmpty(cachedDeviceId))
            {
                cachedDeviceId = GetDeviceIdFromETag();
            }

            return cachedDeviceId;
        }

        private string GetDeviceIdFromCookie(string key)
        {
            if (Request.Cookies.TryGetValue(key, out var deviceId))
            {
                return DeviceFactory.IsValidDeviceId(deviceId) ? deviceId : string.Empty;
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

            var domainName = rawUrl.Host == "localhost" ? rawUrl.Host + ":" + rawUrl.Port : rawUrl.Host;


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
