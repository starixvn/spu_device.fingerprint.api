namespace Device.Identification.Services
{
    using Device.Identification.Factory;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using System;

    public interface IHttpCachingWrapper
    {
        void UpdateCachingWithExpiredSeconds(int expiredSeconds);

        bool DoesRecoverDeviceIdFromETag();
    }

    public class HttpCachingWrapper(IHttpContextAccessor contextAccessor) : IHttpCachingWrapper
    {
        private string _etag = string.Empty;

        private HttpRequest? Request => contextAccessor.HttpContext?.Request;
        private HttpResponse? Response => contextAccessor.HttpContext?.Response;

        public string ETag
        {
            get => _etag
                ?? Request?.Headers[HeaderNames.IfNoneMatch].ToString()
                ?? string.Empty;

            set
            {
                if (_etag == value)
                    return;

                _etag = value;

                if (Response != null && !Response.HasStarted)
                {
                    Response.Headers[HeaderNames.ETag] = value;
                }
            }
        }

        public void UpdateCachingWithExpiredSeconds(int expiredSeconds)
        {
            if (Response != null && !Response.HasStarted && expiredSeconds > 0)
            {
                SetCacheExpiration(TimeSpan.FromSeconds(expiredSeconds));
            }
        }



        public bool DoesRecoverDeviceIdFromETag() =>
            DeviceFactory.IsValidDeviceId(ETag);

        private void SetCacheExpiration(TimeSpan maxAge)
        {
            var headers = Response?.GetTypedHeaders();
            if (headers != null)
            {
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Private = true,
                    MaxAge = maxAge
                };
            }
        }
    }

}