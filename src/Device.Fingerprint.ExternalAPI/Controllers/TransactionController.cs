using Device.Fingerprint.ExternalAPI.Models;
using Device.FingerPrint.Core;
using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Device.Fingerprint.WebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {

        private readonly IWebHostEnvironment environment;
        private readonly IAppSettings appSettings;

        private readonly ISubscriberServices subscriberServices;
        private readonly ITransactionService transactionService;
        private JWTService jWTService;
        public TransactionController(
            IWebHostEnvironment Environment,
            IAppSettings appSettings,
            ISubscriberServices subscriberServices,
            ITransactionService transactionService)
        {
            this.environment = Environment;
            this.appSettings = appSettings;
            this.subscriberServices = subscriberServices;
            this.transactionService = transactionService;
            jWTService = new JWTService(appSettings);

        }               

        /// <summary>
        /// Get transaction history by range date and subscriber
        /// </summary>
        /// <param name="subscriberName"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        [HttpGet("GetTransactionHistory/{subscriberName}/{fromDate}")]
        public ActionResult<TransactionHistoryList> GetTransactionHistory(string subscriberName, DateTime fromDate)
        {

            try
            {
                var subscriber = subscriberServices.GetSubscriberByName(subscriberName);

                if (subscriber != null)
                {
                    var trans = transactionService.GetTransactionHistoryByDateRangeSubscriber(subscriber.SubscriberId, fromDate, 1000);

                    var transformedTrans = trans.Select(x => new TransactionHistoryModel
                    {
                        Action = x.Action,
                        Browser = x.Browser,
                        Domain = x.UrlDetails,
                        Device = x.FirstDeviceCode,
                        IP = x.IP,
                        DeviceType = x.OS,                        
                        RobotTracking = x.RobotTracking,
                        TrackingNumber = x.TransID,
                        DateTime = x.TransTime,
                        Account = x.UserName,                        
                        SubscriberID = x.SubscriberID,
                        RawFingerPrintID = x.RawFingerPrintID,
                        RawUserAgentID = x.RawUserAgentID
                    });

                    return Ok(new TransactionHistoryList
                    {
                        Trans = transformedTrans,
                        ErrorCode = 0,
                        ErrorMessage = string.Empty                       
                    });
                }
                else
                {
                    return Ok(new TransactionHistoryList
                    {
                        Trans = null,
                        ErrorCode = -1,
                        ErrorMessage = "Subscriber cannot be null"                       
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new TransactionHistoryList
                {
                    Trans = null,
                    ErrorCode = -1,
                    ErrorMessage = ex.Message
                });
            }       
        }

        /// <summary>
        /// Get association list by range date and subscriber
        /// </summary>
        /// <param name="subscriberName"></param>
        /// <param name="fromDate"></param>
        /// <returns></returns>
        [HttpGet("GetAssociation/{subscriberName}/{fromDate}")]
        public ActionResult<AssociationList> GetAssociation(string subscriberName, DateTime fromDate)
        {
            try
            {
                var subscriber = subscriberServices.GetSubscriberByName(subscriberName);

                if (subscriber != null)
                {
                    var associationList = transactionService.GetAssociationByDateRangeSubscriber(subscriber.SubscriberId, fromDate, 1000);

                    var transformedAss = associationList.Select(x => new AssociationModel
                    {
                       Account = x.LoginName,
                       AccountID = x.AccountID,
                       AssociationID = x.AssociationID,
                       CreatedTime = x.CreatedTime,
                       DeviceCode = x.FirstDeviceCode,
                       DeviceID = x.DeviceID
                    });

                    return Ok(new AssociationList
                    {
                        Associations = transformedAss,
                        ErrorCode = 0,
                        ErrorMessage = string.Empty                      

                    });
                }
                else
                {
                    return Ok(new AssociationList
                    {
                        Associations = null,
                        ErrorCode = -1,
                        ErrorMessage = "Subscriber cannot be null"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new TransactionHistoryList
                {
                    Trans = null,
                    ErrorCode = -1,
                    ErrorMessage = ex.Message
                });
            }
        }


        [HttpGet("generatetoken/audience")]
        [AllowAnonymous]
        public string GenerateToken(string audience, string issue)
        {
            return jWTService.GenerateJWT(issue, audience, DateTime.UtcNow.AddYears(10));
        }
    }
}
