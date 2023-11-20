namespace Device.FingerPrint.Core.Filters
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Device.FingerPrint.Core.Model;
    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class LogTrailAttribute : Attribute, IAsyncActionFilter
    {
        private readonly IAppSettings appSettings;
        private readonly Fanex.Logging.ILogger logger;
        private readonly Serilog.ILogger serilog;

        public LogTrailAttribute(
            Fanex.Logging.ILogger logger,
            Serilog.ILogger serilog,
            IAppSettings appSettings
            )
        {
            this.logger = logger;
            this.serilog = serilog;
            this.appSettings = appSettings;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                await next();

                if (context.HttpContext.Request.Method == "POST")
                {
                    var model = context.ActionArguments.FirstOrDefault().Value;
                    var urlAddress = new Uri(model.GetType().GetProperty("Url")?.GetValue(model, null)?.ToString());

                    var logEvent = new LogEventInfo
                    {
                        SubscriberID = int.Parse(model.GetType().GetProperty("SubscriberId")?.GetValue(model, null).ToString()),
                        SystemName = appSettings.SystemName,
                        EventDate = DateTime.Now,
                        Source = $"{urlAddress.Host}:{urlAddress.Port}",
                        Destination = context.HttpContext.Request.Host.ToString(),
                        Method = context.HttpContext.Request.Path.ToString(),
                    };

                    var logEventJson = JsonSerializer.Serialize(logEvent);

                    serilog.Information(logEventJson);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }
}
