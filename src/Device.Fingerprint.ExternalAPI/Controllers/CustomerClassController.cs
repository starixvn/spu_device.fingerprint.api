using Device.Fingerprint.ExternalAPI.Models;
using Device.FingerPrint.Features.RiskLevel.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Device.Fingerprint.ExternalAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class CustomerClassController : ControllerBase
    {        
        private readonly ISubscriberServices subscriberServices;
        private readonly IRiskLevelService riskLevelService;
        
        public CustomerClassController(     
            ISubscriberServices subscriberServices,
            IRiskLevelService riskLevelService)
        {            
            this.subscriberServices = subscriberServices;
            this.riskLevelService = riskLevelService;
        }


        [HttpGet("GetRiskLevelBySubscriber/{subscriberName}/{lastId}")]
        public ActionResult<CustomerClassRiskLevelModel> GetRiskLevelBySubscriber(string subscriberName, long lastId)
        {
            try
            {
                var subscriber = subscriberServices.GetSubscriberByName(subscriberName);

                if (subscriber != null)
                {
                    var trans = riskLevelService.GetCustomerClassRiskLevel(subscriber.SubscriberId, lastId, 1000);

                    var transformedTrans = trans.Select(x => new CustomerClassRiskLevelModel
                    {
                       Account = x.RegisterName,
                       Id = x.Id,
                       RiskLevel = x.RiskLevel,
                    });

                    return Ok(new CustomerClassRiskLevelList
                    {
                        Trans = transformedTrans,
                        StatusCode = System.Net.HttpStatusCode.OK,
                        ErrorMessage = string.Empty
                    });
                }
                else
                {
                    return Ok(new CustomerClassRiskLevelList
                    {
                        Trans = null,
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        ErrorMessage = "Subscriber cannot be null"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new CustomerClassRiskLevelList
                {
                    Trans = null,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
