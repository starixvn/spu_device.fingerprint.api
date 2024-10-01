using Device.Fingerprint.ExternalAPI.Filters;
using Device.Fingerprint.ExternalAPI.Models;
using Device.FingerPrint.Core;
using Device.FingerPrint.Features.RiskLevel.Model;
using Device.FingerPrint.Features.RiskLevel.Services;
using Device.FingerPrint.Features.Subscriber.Services;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.Fingerprint.WebAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private static readonly string yyyyMMddFormat = "yyyy-MM-dd";

        private readonly ISubscriberServices subscriberServices;
        private readonly ITransactionService transactionService;
        private readonly IRiskLevelService riskLevelService;
        private readonly IAppSettings appSettings;

        public TransactionController(          
            ISubscriberServices subscriberServices,
            ITransactionService transactionService,
            IRiskLevelService riskLevelService,
            IAppSettings appSettings)
        {
            this.subscriberServices = subscriberServices;
            this.transactionService = transactionService; 
            this.riskLevelService = riskLevelService;
            this.appSettings = appSettings;
        }
        
        [HttpGet("GetTransactionBySubscriber/{subscriberName}/{trackingNumber}")]
        [ServiceFilter(typeof(RequestCounterFilter))]
        public ActionResult<TransactionHistoryList> GetTransactionHistory(string subscriberName, long trackingNumber)
        {

            try
            {
                var subscriber = subscriberServices.GetSubscriberByName(subscriberName);

                if (subscriber != null)
                {
                    var trans = transactionService.GetTransactionHistoryByTransIdSubscriber(subscriber.SubscriberId, trackingNumber, 1000);

                    IEnumerable<RiskLevelByAccountEntity> riskLevels = null;

                    var accountIDs = string.Join(',', trans.Select(x => x.AccountID).Distinct());

                    if (!string.IsNullOrEmpty(accountIDs))
                        riskLevels = riskLevelService.GetCustomerClassRiskLevelByAccount(accountIDs);

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
                        RiskLevel = riskLevels?.FirstOrDefault(y => y.AccountID == x.AccountID)?.RiskLevel ?? string.Empty
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
        [ServiceFilter(typeof(RequestCounterFilter))]
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

        [HttpGet("GetAssociationByDevice/{subscriberName}/{device}/{fromDate}/{toDate}")]
        [ServiceFilter(typeof(RequestCounterFilter))]
        public ActionResult<AssociationAccountUsageList> GetAssociationByDevice(string subscriberName, string device, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (DateTime.Compare(fromDate, toDate) > 0)
                {
                    return Ok(new AssociationAccountUsageList
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ErrorMessage = "ToDate cannot be less than FromDate"
                    });
                }

                if (!IsDateRangeValid(fromDate, toDate))
                {
                    return Ok(new AssociationAccountUsageList
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ErrorMessage = "Invalid input: fromDate and toDate must be within " + appSettings.MaxMonthsGetAssociationByDevice + " months from the current date."
                    });
                }

                var subscriber = subscriberServices.GetSubscriberByName(subscriberName);

                if (subscriber != null)
                {
                    var associationBySubscriberDevice = transactionService.GetAssociationBySubscriberDevice(subscriber.SubscriberId, device, fromDate, toDate);

                    return Ok(new AssociationAccountUsageList
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        ErrorMessage = string.Empty,
                        FromDate = fromDate.ToString(yyyyMMddFormat),
                        ToDate = toDate.ToString(yyyyMMddFormat),
                        AssociatedAccounts = associationBySubscriberDevice.Count(),
                        AssociatedAccountDetails = associationBySubscriberDevice.Select(x => new AssociationAccountUsageModel { 
                                                                                                    Account = x.LoginName,
                                                                                                    MinUsageDate = x.MnUsageDate.ToString(yyyyMMddFormat),
                                                                                                    MaxUsageDate = x.MaxUsageDate.ToString(yyyyMMddFormat),
                                                                                                    UsageCount = x.UsageCount }),
                    });
                }
                else
                {
                    return Ok(new AssociationAccountUsageList
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        ErrorMessage = "Subscriber cannot be null"
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new AssociationAccountUsageList
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = ex.Message
                });
            }
        }

        private bool IsDateRangeValid(DateTime fromDate, DateTime toDate)
        {
            DateTime minValidDate = DateTime.Today.AddMonths(- appSettings.MaxMonthsGetAssociationByDevice);

            return DateTime.Compare(minValidDate, fromDate) <= 0 && DateTime.Compare(fromDate, DateTime.Today) <= 0
                        && DateTime.Compare(minValidDate, toDate) <= 0 && DateTime.Compare(toDate, DateTime.Today) <= 0;
        }

    }
}
