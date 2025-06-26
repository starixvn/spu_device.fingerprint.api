using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Device.Fingerprint.InternalAPI.Controllers
{
    [Route("api/subscriber")]
    [ApiController]
    public class SubscriberController(ISubscriberServices subscriberServices) : ControllerBase
    {
        private readonly ISubscriberServices subscriberServices = subscriberServices;

        [HttpGet("GetSubscriberInfo")]
        [AllowAnonymous]
        public ActionResult<SubscriberModel> GetSubscriberInfo(string subscriberName)
        {
            var result = subscriberServices.GetSubscriberByName(subscriberName);

            if (result == null)
            {
                return Ok(new SubscriberModel());
            }

            return Ok(result);
        }
    }
}
