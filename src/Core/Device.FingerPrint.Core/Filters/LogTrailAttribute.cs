namespace Device.FingerPrint.Core.Filters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Device.FingerPrint.Core.Model;
    using Device.FingerPrint.Core.Services;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class LogTrailAttribute : Attribute, IAsyncActionFilter
    {
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;
        private readonly ILogTrailClientService logTrailClientService;

        public LogTrailAttribute(
            IAppSettings appSettings,
            ILogger logger,
            ILogTrailClientService logTrailClientService
            )
        {
            this.appSettings = appSettings;
            this.logger = logger;
            this.logTrailClientService = logTrailClientService;
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
                        SubscriberID = int.Parse(model.GetType().GetProperty("SubscriberId")?.GetValue(model, null).ToString()),
                        SystemName = appSettings.SystemName,
                        EventDate = DateTime.Now,
                        Source = model.GetType().GetProperty("Url")?.GetValue(model, null)?.ToString(),
                        Destination = context.HttpContext.Request.Host.ToString(),
                        Method = context.HttpContext.Request.Path.ToString(),
                    };

                    await logTrailClientService.PostAsync(logEvent);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }
    }
}
