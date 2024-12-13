using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Device.Identification
{
    public class AshxMiddleware
    {
        private readonly RequestDelegate _next;

        public AshxMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.Value.EndsWith(".ashx"))
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("This is a response from .ashx handler.");
            }
            else
            {
                await _next(context);
            }
        }
    }
    public static class AshxMiddlewareExtensions
    {
        public static IApplicationBuilder UseAshxMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AshxMiddleware>();
        }
    }
}
