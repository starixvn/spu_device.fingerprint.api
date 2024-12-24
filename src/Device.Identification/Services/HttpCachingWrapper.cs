namespace Device.Identification.Services
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using System;
    using System.Globalization;

    public class HttpCachingWrapper(IHttpContextAccessor contextAccessor, IAppSettings appSettings)
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
        private readonly IAppSettings _appSettings = appSettings;
        private string _etag;

        private HttpRequest Request => _contextAccessor.HttpContext?.Request;
        private HttpResponse Response => _contextAccessor.HttpContext?.Response;

        public DateTime ModifiedTime
        {
            get
            {
                var modifiedTime = Request.Headers[HeaderNames.IfModifiedSince];
                return !string.IsNullOrEmpty(modifiedTime)
                    ? DateTime.Parse(modifiedTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                    : DateTime.MinValue;
            }
            set
            {
                if (value == default)
                {
                    return;
                }

                Response.Headers[HeaderNames.LastModified] = value.ToString("R", CultureInfo.InvariantCulture); // RFC1123 format
            }
        }

        private void SetPrivateCache()
        {
            var headers = Response.GetTypedHeaders();
            headers.CacheControl = new CacheControlHeaderValue
            {
                Private = true
            };
        }

        private void SetExpiredDays(int value)
        {
            var headers = Response.GetTypedHeaders();
            headers.CacheControl = new CacheControlHeaderValue
            {
                Private = true,
                MaxAge = TimeSpan.FromDays(value)
            };
        }

        private void SetExpiredSeconds(int value)
        {
            var headers = Response.GetTypedHeaders();
            headers.CacheControl = new CacheControlHeaderValue
            {
                Private = true,
                MaxAge = TimeSpan.FromSeconds(value)
            };
        }

        public string ETag
        {
            get => _etag ?? Request.Headers[HeaderNames.IfNoneMatch];

            set
            {
                if (_etag == value)
                {
                    return;
                }

                _etag = value;
                Response.Headers[HeaderNames.ETag] = _etag;
            }
        }

        public void ReuseCache()
        {
            Response.StatusCode = StatusCodes.Status304NotModified;
        }

        public virtual bool IsCacheValid()
        {
            return ModifiedTime >= _appSettings.ScriptTime && Factory.DeviceFactory.IsValidDeviceId(ETag);
        }

        public virtual void UpdateCachingWithExpiredDays(int expiredDays)
        {
            SetPrivateCache();
            SetExpiredDays(expiredDays);
        }

        public virtual void UpdateCachingWithExpiredDaysAndModifiedTime(int expiredDays)
        {
            SetPrivateCache();
            ModifiedTime = _appSettings.ScriptTime;
            SetExpiredDays(expiredDays);
        }

        public virtual void UpdateCachingWithExpiredSeconds(int expiredSeconds)
        {
            if (!Response.HasStarted)
            {
                SetPrivateCache();
                SetExpiredSeconds(expiredSeconds);
            }
        }

        public virtual void UpdateCachingWithExpiredSecondsAndModifiedTime(int expiredSeconds)
        {
            SetPrivateCache();
            ModifiedTime = _appSettings.ScriptTime;
            SetExpiredSeconds(expiredSeconds);
        }
    }

}