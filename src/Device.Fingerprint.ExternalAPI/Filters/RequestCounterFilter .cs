using Device.Fingerprint.ExternalAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Device.Fingerprint.ExternalAPI.Filters
{
    public class RequestCounterFilter: IAsyncActionFilter
    {
        private readonly RequestCounterService _counterService;       

        public RequestCounterFilter(RequestCounterService counterService)
        {
            _counterService = counterService;           
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {           
            var actionDescriptor = ((ControllerBase)context.Controller).ControllerContext.ActionDescriptor;
            var actionName = actionDescriptor.ActionName;
            var argument = context.ActionArguments["subscriberName"].ToString().ToLower();

            await _counterService.IncrementRequestCountAsync(argument + ":" + actionName);

            await next();
        }

        
    }
}
