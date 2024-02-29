using Device.Fingerprint.ExternalAPI.Models;
using Device.FingerPrint.Core;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
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
        }

        [HttpGet("GetLatestTransaction/{subscriberName}/{length}/{take}/{skip}")]
        public ActionResult<LatestTransactionList> GetLatestTransaction(string subscriberName, int length, int take, int skip)
        {
            var subscriber = subscriberServices.GetSubscriberByName(subscriberName);
            var totalItem = 0;

            if (subscriber != null)
            {
                var trans = transactionService.GetLatestTransaction(subscriber.SubscriberId, length, take, skip, out totalItem);

                var transformedTrans = trans.Select(x => new LatestTransactionModel {
                    Action = x.Action,
                    Browser = x.Browser,
                    Domain = x.UrlDetail,
                    Device = x.FirstDeviceCode,
                    IP = x.IP,
                    OS = x.OS,
                    RegisterName = x.RegisterName,
                    RobotTracking = x.RobotTracking,
                    TransID = x.TransID,    
                    TransTime = x.TransTime,
                    UserName = x.UserName
                });

                return Ok(new LatestTransactionList
                {
                    Length = length,
                    TotalItem = totalItem,
                    Take = take,
                    Skip = skip,
                    Trans = transformedTrans
                });
            }

            return Ok(new LatestTransactionList
            {
                Length = length,
                TotalItem = totalItem,
                Take = take,
                Skip = skip,
                Trans = null
            });
        }

        [HttpGet("GetTransactionHistory/{loginName}/{subscriberName}/{fromDate}/{toDate}/{take}/{skip}")]
        public ActionResult<TransactionHistoryList> GetTransactionHistory(string loginName, string subscriberName, DateTime fromDate, DateTime toDate, int take, int skip)
        {

            var subscriber = subscriberServices.GetSubscriberByName(subscriberName);
            var totalItem = 0;

            if (subscriber != null)
            {
                var trans = transactionService.GetTransactionHistory(subscriber.SubscriberId, fromDate, toDate, loginName,false, skip, take, out totalItem);

                var transformedTrans = trans.Select(x => new TransactionHistoryModel
                {
                    Action = x.Action,
                    Browser = x.Browser,
                    Domain = x.UrlDetail,
                    Device = x.FirstDeviceCode,
                    IP = x.IP,
                    OS = x.OS,
                    RegisterName = x.RegisterName,
                    RobotTracking = x.RobotTracking,
                    TransID = x.TransID,
                    TransTime = x.TransTime,
                    UserName = x.UserName,
                    CTSCustID = x.CTSCustID,
                    SubscriberID = x.SubscriberID,
                    ActionResult = x.ActionResult
                });

                return Ok(new TransactionHistoryList
                {                   
                    TotalItem = totalItem,
                    Take = take,
                    Skip = skip,
                    Trans = transformedTrans
                });
            }

            return Ok(new TransactionHistoryList
            {                
                TotalItem = totalItem,
                Take = take,
                Skip = skip,
                Trans = null
            });
        }
    }
}
