namespace Device.Identification.Services
{
    using Device.Identification.Factory;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using System;

    public interface IHttpCachingWrapper
    {
        void UpdateCachingWithExpiredDays(int expiredDays);

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
            get => _etag ?? Request?.Headers[HeaderNames.IfNoneMatch].ToString() ?? string.Empty;

            set
            {
                if (_etag == value)
                {
                    return;
                }

                _etag = value;
                if (Response != null)
                {
                    Response.Headers[HeaderNames.ETag] = value;
                }
            }
        }

        public void UpdateCachingWithExpiredDays(int expiredDays)
        {
            if (Response != null && !Response.HasStarted)
            {
                SetExpiredDays(expiredDays);
            }
        }

        public void UpdateCachingWithExpiredSeconds(int expiredSeconds)
        {
            if (Response != null && !Response.HasStarted)
            {
                SetExpiredSeconds(expiredSeconds);
            }
        }

        public bool DoesRecoverDeviceIdFromETag()
        {
            return DeviceFactory.IsValidDeviceId(ETag);
        }

        private void SetExpiredDays(int value)
        {
            if (Response != null && !Response.HasStarted)
            {
                var headers = Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Private = true,
                    MaxAge = TimeSpan.FromDays(value)
                };
            }
        }

        private void SetExpiredSeconds(int value)
        {
            if (Response != null && !Response.HasStarted)
            {
                var headers = Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Private = true,
                    MaxAge = TimeSpan.FromSeconds(value)
                };
            }
        }
    }

}