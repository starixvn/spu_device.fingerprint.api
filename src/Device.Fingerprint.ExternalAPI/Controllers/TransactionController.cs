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
        
        [HttpGet("GetTransactionBySubscriber/{subscriberName}/{trackingNumber}")]
        public ActionResult<TransactionHistoryList> GetTransactionHistory(string subscriberName, long trackingNumber)
        {

            try
            {
                var subscriber = subscriberServices.GetSubscriberByName(subscriberName);

                if (subscriber != null)
                {
                    var trans = transactionService.GetTransactionHistoryByTransIdSubscriber(subscriber.SubscriberId, trackingNumber, 1000);

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
                        Account = x.UserName
                    });

                    return Ok(new TransactionHistoryList
                    {
                        Trans = transformedTrans,
                        StatusCode = System.Net.HttpStatusCode.OK,
                        ErrorMessage = string.Empty
                    });
                }
                else
                {
                    return Ok(new TransactionHistoryList
                    {
                        Trans = null,
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        ErrorMessage = "Subscriber cannot be null"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new TransactionHistoryList
                {
                    Trans = null,
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = ex.Message
                });
            }
        }
        
        [HttpGet("GetAssociationByAccount/{subscriberName}/{account}")]
        public ActionResult<AssociationList> GetAssociationByAccount(string subscriberName, string account)
        {
            try
            {
                var subscriber = subscriberServices.GetSubscriberByName(subscriberName);

                if (subscriber != null)
                {
                    var associationByAccountSubscriber = transactionService.GetAssociationByAccountSubscriber(subscriber.SubscriberId, account);

                    return Ok(new AssociationList
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        ErrorMessage = string.Empty,
                        AssociatedDevices = associationByAccountSubscriber.Item1.Count(),
                        AssociatedDeviceDetails = associationByAccountSubscriber.Item1.Select(x => new AssociationDeviceModel { Device = x.Device, FirstSeenDate = x.FirstSeenDate }),
                        AssociatedAccounts = associationByAccountSubscriber.Item2.Count(),
                        AssociatedAccountDetails = associationByAccountSubscriber.Item2.Select(x => new AssociationAccountModel { Account = x.Account, FirstAssociationDate = x.FirstAssociationDate }),

                    });
                }
                else
                {
                    return Ok(new AssociationList
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        ErrorMessage = "Subscriber cannot be null"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new AssociationList
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = ex.Message
                });
            }
        }


        [HttpGet("generatetoken/audience")]
        [AllowAnonymous]
        public string GenerateToken(string audience)
        {
            return jWTService.GenerateJWT(DateTime.UtcNow.AddYears(10), audience);
        }
    }
}
