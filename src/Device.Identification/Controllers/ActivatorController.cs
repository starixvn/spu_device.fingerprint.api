namespace Device.Identification.Controllers
{
    using Device.Identification.ActivatorScript;
    using Device.Identification.Factory;
    using Device.Identification.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using NUglify;
    using System;
    using System.Collections.Generic;

    [ApiController]
    public class ActivatorController(IAppSettings appSettings
                                , IEcService ecService
                                , IRecoverDeviceService recoverDeviceService
                                , ActivatorScriptFactory scriptFactory
                                , HttpCachingWrapper httpCachingWrapper
                                , EverCookieCachingWrapper cacheWrapper
    ) : ControllerBase
    {
        private readonly IAppSettings _appSettings = appSettings;
        private readonly IEcService _ecService = ecService;
        private readonly IRecoverDeviceService _recoverDeviceService = recoverDeviceService;
        private readonly ActivatorScriptFactory _scriptFactory = scriptFactory;
        private readonly HttpCachingWrapper _httpCachingWrapper = httpCachingWrapper;
        private readonly EverCookieCachingWrapper _cacheWrapper = cacheWrapper;

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
                _httpCachingWrapper.UpdateCachingWithExpiredSeconds(0);
                await WriteActivatorContent(cachedDeviceId, defaultDeviceId, fdiKey);
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

        [HttpGet("comm.ashx")]
        public async Task CommAshx()
        {
            try
            {
                var subscriberScript = _scriptFactory.CreateSubscriberCommonScript();
                _httpCachingWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);

                await subscriberScript.WriteScript(Response);
            }
            catch (Exception ex)
            {
                var err = ex.Message;
            }
        }

        [HttpGet("ec.ashx")]
        public async Task EcAshx()
        {
            try
            {
                if (string.IsNullOrEmpty(Request.Query["deviceInfo"]))
                {
                    await ResponseEmptyContent();
                }

                var requestIp = _ecService.GetRequestIpFromClientToParseDeviceInfo();
                var deviceDetection = _ecService.ParseDeviceInfo(requestIp);
                var candidateDeviceIds = _ecService.GetCandidatePlainDeviceIdsFromStorages(deviceDetection);
                var currentDeviceCode = _ecService.GetDeviceCode(deviceDetection);

                if (_cacheWrapper.IsCacheValid(deviceDetection?.Device?.Code))
                {
                    _cacheWrapper.UpdateCachingWithExpiredDays(30);
                    _cacheWrapper.ReuseCache();
                    return;
                }

                deviceDetection.RemoteIPs = Request.Headers["X-Forwarded-For"];
                var recoveredDevice = _recoverDeviceService.GetRecoveredDevice(candidateDeviceIds, currentDeviceCode);

                _ecService.TryInsertNewDevice(recoveredDevice, deviceDetection);
                if (!Response.HasStarted)
                {
                    _ecService.SynchronizeServerStorages(recoveredDevice, _cacheWrapper);
                    _ecService.UpdateCachingWithExpiredDaysAndModifiedTime(30);
                }

                await ResponseInfoToClient(recoveredDevice?.DeviceId);

            }
            catch (Exception ex)
            {
                await ResponseEmptyContent();
            }
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

                if (!Response.HasStarted)
                {
                    Response.Headers[HeaderNames.ETag] = cachedDeviceId;
                }
            }
        }

        private async Task WriteActivatorContent(string cachedDeviceId, string defaultDeviceId, string fdiKey)
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

        private async Task ResponseEmptyContent()
        {
            var content = string.Empty;
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
