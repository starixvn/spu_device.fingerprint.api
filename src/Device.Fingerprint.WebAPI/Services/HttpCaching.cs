using Microsoft.AspNetCore.Http;

namespace Device.Fingerprint.WebAPI.Services
{
    public class HttpCaching
    {
        private string etag;
        private readonly HttpRequest request;
        private readonly HttpResponse response;

        public HttpCaching(HttpContext context)
        {
            request = context.Request;
            response = context.Response;
        }

        public string Etag {
            get
            {
                return etag ?? request.Headers["If-None-Match"];
            }

            set
            {
                if (etag == value)
                {
                    return;
                }

                etag = value;

                response.Headers.Add("ETag", new[] { etag });
            }
        }
    }
}
