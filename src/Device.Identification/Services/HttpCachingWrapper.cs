namespace Device.Identification.Services
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Net.Http.Headers;
    using System;

    public interface IHttpCachingWrapper
    {
        void ReuseCache();

        void UpdateCachingWithExpiredDays(int expiredDays);

        void UpdateCachingWithExpiredSeconds(int expiredSeconds);
    }

    public class HttpCachingWrapper(IHttpContextAccessor contextAccessor
                                , IAppSettings appSettings) : IHttpCachingWrapper
    {
        private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
        private string _etag;

        private HttpRequest Request => _contextAccessor.HttpContext?.Request;
        private HttpResponse Response => _contextAccessor.HttpContext?.Response;

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

        public void UpdateCachingWithExpiredDays(int expiredDays)
        {
            if (!Response.HasStarted)
            {
                SetExpiredDays(expiredDays);
            }
        }

        public void UpdateCachingWithExpiredSeconds(int expiredSeconds)
        {
            if (!Response.HasStarted)
            {
                SetExpiredSeconds(expiredSeconds);
            }
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
    }

}