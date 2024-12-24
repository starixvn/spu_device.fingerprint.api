namespace Device.Identification.Services
{
    using Device.Identification.Factory;
    using Microsoft.AspNetCore.Http;
    using System.Globalization;

    public class EverCookieCachingWrapper(IHttpContextAccessor contextAccessor, IAppSettings appSettings) : HttpCachingWrapper(contextAccessor, appSettings)
    {
        private readonly IAppSettings _appSettings = appSettings;
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;

        public bool IsCacheValid(string deviceCode)
        {
            return DoesRecoverDeviceIdFromETag()
                && WasDeviceCodeNotChanged(deviceCode);
        }

        private bool DoesRecoverDeviceIdFromETag()
        {
            var deviceId = ETag;
            return DeviceFactory.IsValidDeviceId(deviceId);
        }

        private bool WasDeviceCodeNotChanged(string deviceCode)
        {
            var cookies = _contextAccessor.HttpContext?.Request.Cookies;

            if (cookies == null || !cookies.TryGetValue(_appSettings.CookieVersion, out var currentCookie))
            {
                return false;
            }

            return string.Compare(currentCookie, deviceCode, true, CultureInfo.InvariantCulture) == 0;
        }
    }

}
