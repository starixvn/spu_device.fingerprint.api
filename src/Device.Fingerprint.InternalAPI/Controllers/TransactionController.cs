using Device.Fingerprint.InternalAPI.Model;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Device.Fingerprint.InternalAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class TransactionController(ITransactionService transactionService) : ControllerBase
    {
        private readonly ITransactionService transactionService = transactionService;

        [HttpGet("GetTransactionInfo/{lastTransID}/{batchSize}")]
        [EnableRateLimiting("fixed")]
        public ActionResult<TransactionInfoModel> GetTransactionInfo(ulong lastTransID, int batchSize)
        {
            try
            {
                if (lastTransID < 0)
                {
                    return Ok(new TransactionInfoResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ErrorMessage = "lastTransID cannot be less than 0!"
                    });
                }

                if (batchSize > 10000)
                {
                    return Ok(new TransactionInfoResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        ErrorMessage = "batchSize cannot be greater than 10000!"
                    });
                }

                var transactionInfos = transactionService.GetTransactionInfo(lastTransID, batchSize);

                if (transactionInfos != null && transactionInfos.Any())
                {
                    var transformedTrans = transactionInfos.Select(x => new TransactionInfoModel
                    {
                        AccountID = x.AccountID,
                        Action = x.Action,
                        ActionResult = x.ActionResult,
                        ActionResultID = x.ActionResultID,
                        BotComponentID = x.BotComponentID,
                        BotDetectionValue = x.BotDetectionValue,
                        BrowserName = x.BrowserName,
                        BrowserVersion = x.BrowserVersion,
                        CreatedDate = x.CreatedDate,
                        CustID = x.CustID,
                        DeviceCode = x.DeviceCode,
                        DeviceFingerprintID = x.DeviceFingerprintID,
                        DeviceID = x.DeviceID,
                        DeviceStatus = x.DeviceStatus,
                        DeviceType = x.DeviceType,
                        FakeIP = x.FakeIP,
                        FingerprintCode = x.FingerprintCode,
                        FirstDeviceCode = x.FirstDeviceCode,
                        Flagged = x.Flagged,
                        InsertTime = x.InsertTime,
                        IP = x.IP,
                        IPID = x.IPID,
                        loginName = x.loginName,
                        OSName = x.OSName,
                        OSVersion = x.OSVersion,
                        RobotTracking = x.RobotTracking,
                        SubscriberID = x.SubscriberID,
                        SubscriberName = x.SubscriberName,
                        TransID = x.TransID,
                        TransStatus = x.TransStatus,
                        TransTime = x.TransTime,
                        URLDetails = x.URLDetails,
                        URLID = x.URLID,
                        UserAgent = x.UserAgent,
                        UserAgentKey = x.UserAgentKey
                    });

                    return Ok(new TransactionInfoResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        ErrorMessage = string.Empty,
                        TransactionInfo = transformedTrans
                    });
                }

                return Ok(new TransactionInfoResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ErrorMessage = string.Empty
                });
            }
            catch (Exception ex)
            {
                return Ok(new TransactionInfoResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = ex.Message
                });
            }
        }


        [HttpGet(template: "TransactionValidateSuccess/{loginName}/{transTime}")]
        [AllowAnonymous]
        public ActionResult<TransactionValidateSuccessEntity> TransactionValidateSuccess(string loginName, DateTime transTime)
        {
            var transaction = transactionService.TransactionValidateSuccess(loginName, transTime);

            return Ok(transaction);
        }
    }
}