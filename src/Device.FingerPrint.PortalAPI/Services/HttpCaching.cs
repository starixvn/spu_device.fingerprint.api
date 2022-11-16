using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.PortalAPI.Services
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
                return etag ?? request.Headers[HeaderNames.IfNoneMatch];
            }

            set
            {
                if (etag == value)
                {
                    return;
                }

                etag = value;

                response.Headers.Add(HeaderNames.ETag, new[] { etag });
            }
        }
    }
}
