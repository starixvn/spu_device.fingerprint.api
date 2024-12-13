namespace Device.Identification.Services
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using System;

    public class HttpCachingWrapper
    {
        private readonly HttpRequest _request;
        private readonly HttpResponse _response;
        private readonly IAppSettings _appSettings;
        private string _etag;
        private DateTime? _modifiedTime;

        public HttpCachingWrapper(IHttpContextAccessor contextAccessor, IAppSettings appSettings)
        {
            if (contextAccessor.HttpContext == null)
            {
                throw new ArgumentNullException(nameof(contextAccessor.HttpContext), "HttpContext is not available.");
            }

            _request = contextAccessor.HttpContext.Request;
            _response = contextAccessor.HttpContext.Response;
            _appSettings = appSettings;
        }

        private DateTime ModifiedTime
        {
            get
            {
                if (_modifiedTime.HasValue)
                {
                    return _modifiedTime.Value;
                }

                var modifiedTime = _request.Headers[HeaderNames.IfModifiedSince];
                return !string.IsNullOrEmpty(modifiedTime) ? Convert.ToDateTime(modifiedTime) : DateTime.MinValue;
            }

            set
            {
                if (_modifiedTime == value || value == default)
                {
                    return;
                }

                _modifiedTime = value;
                _response.Headers[HeaderNames.LastModified] = _modifiedTime.Value.ToString("R"); // RFC1123 format
            }
        }

        private void SetPrivateCache()
        {
            var headers = _response.GetTypedHeaders();
            headers.CacheControl = new CacheControlHeaderValue
            {
                Private = true
            };
        }

        private void SetExpiredDays(int value)
        {
            var headers = _response.GetTypedHeaders();
            headers.CacheControl = new CacheControlHeaderValue
            {
                Private = true,
                MaxAge = TimeSpan.FromDays(value)
            };
        }

        private void SetExpiredSeconds(int value)
        {
            var headers = _response.GetTypedHeaders();
            headers.CacheControl = new CacheControlHeaderValue
            {
                Private = true,
                MaxAge = TimeSpan.FromSeconds(value)
            };
        }

        public string ETag
        {
            get => _etag ?? _request.Headers[HeaderNames.IfNoneMatch];

            set
            {
                if (_etag == value)
                {
                    return;
                }

                _etag = value;
                _response.Headers[HeaderNames.ETag] = _etag;
            }
        }

        public void ReuseCache()
        {
            _response.StatusCode = StatusCodes.Status304NotModified;
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
            SetPrivateCache();
            SetExpiredSeconds(expiredSeconds);
        }

        public virtual void UpdateCachingWithExpiredSecondsAndModifiedTime(int expiredSeconds)
        {
            SetPrivateCache();
            ModifiedTime = _appSettings.ScriptTime;
            SetExpiredSeconds(expiredSeconds);
        }
    }

}