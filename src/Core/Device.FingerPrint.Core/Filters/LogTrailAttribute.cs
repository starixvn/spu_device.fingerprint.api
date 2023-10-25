namespace Device.FingerPrint.Core.Filters
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Device.FingerPrint.Core.Model;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Text.Json;
    using Microsoft.Extensions.Configuration;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class LogTrailAttribute : Attribute, IAsyncActionFilter
    {
        private readonly ILogger logger;
        private readonly IAppSettings appSettings;

        public LogTrailAttribute(IAppSettings appSettings, ILogger logger)
        {
            this.appSettings = appSettings;
            this.logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                await next();

                if (context.HttpContext.Request.Method == "POST")
                {
                    var model = context.ActionArguments.FirstOrDefault().Value;

                    var logEvent = new LogEventInfo
                    {
                        SubscriberName = model.GetType().GetProperty("SubscriberName")?.GetValue(model, null)?.ToString(),
                        EventTime = DateTime.Now,
                        Source = model.GetType().GetProperty("Url")?.GetValue(model, null)?.ToString(),
                        Destination = context.HttpContext.Request.Host.ToString(),
                        Method = context.HttpContext.Request.Path.ToString()
                    };

                    await PostBasicAsync(logEvent);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private async Task PostBasicAsync(LogEventInfo logEventInfo)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, appSettings.ApiUrl))
            {
                var json = JsonSerializer.Serialize(logEventInfo);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, new CancellationToken())
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }

}
