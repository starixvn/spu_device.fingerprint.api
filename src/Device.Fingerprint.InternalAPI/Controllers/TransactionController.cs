namespace Device.Fingerprint.InternalAPI.Controllers
{
    using Device.Fingerprint.InternalAPI.Model;
    using Device.FingerPrint.Features.Transaction.Model;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.RateLimiting;
    using Newtonsoft.Json;

    [Route("api")]
    [ApiController]
    public class TransactionController(ITransactionService transactionService, ILogger logger) : ControllerBase
    {
        private readonly ITransactionService transactionService = transactionService;
        private readonly ILogger logger = logger;

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

        [HttpPost("GetTransactionHistory")]
        [AllowAnonymous]
        public ActionResult<TransactionResultModel> GetTransactionHistory([FromBody] TransactionParamModel param)
        {
            try
            {
                var data = transactionService.GetTransactionHistory(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Transaction History exception: " + ex.Message);

                if (!string.IsNullOrEmpty(ex.Message) && ex.Message.ToLower().Contains("timeout expired"))
                {
                    return Ok(new TransactionResultModel() { IsTimeout = true });
                }

                return Ok(new TransactionResultModel());
            }
        }

        [HttpPost("GetLastestTransactionBySubscriber")]
        [AllowAnonymous]
        public ActionResult<LatestTransactionResultModel> GetLastestTransactionBySubscriber([FromBody] LatestTransactionParamModel param)
        {
            try
            {
                var data = transactionService.GetLastestTransactionBySubscriber(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Lastest Transaction By Subscriber exception: " + ex.Message);
                return Ok(new LatestTransactionResultModel());
            }
        }

        [HttpPost("GetTransactionHistoryByDevice")]
        [AllowAnonymous]
        public ActionResult<TransactionHistoryByDeviceResultModel> GetTransactionHistoryByDevice([FromBody] TransactionHistoryByDeviceParamModel param)
        {
            try
            {
                var data = transactionService.GetTransactionHistoryByDevice(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Transaction History By Device exception: " + ex.Message);

                if (!string.IsNullOrEmpty(ex.Message) && ex.Message.ToLower().Contains("timeout expired"))
                {
                    return Ok(new TransactionHistoryByDeviceResultModel() { IsTimeout = true });
                }

                return Ok(new TransactionHistoryByDeviceResultModel());
            }
        }

        [HttpPost("GetTransactionByLoginName")]
        [AllowAnonymous]
        public ActionResult<List<TransactionByLoginNameResultModel>> GetTransactionByLoginName([FromBody] TransactionByLoginNameParamModel param)
        {
            try
            {
                var transactionByLoginName = transactionService.GetTransactionByLoginName(param);

                if (transactionByLoginName != null && transactionByLoginName.Any())
                {
                    var transformedTrans = transactionByLoginName.Select(x =>
                    {
                        var attributesDictionary = DeserializeToDictionary(x.Attribute);

                        var transformedAttributes = TransformFPFingerPrintInfo(attributesDictionary);

                        return new TransactionByLoginNameResultModel
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
                            LoginName = x.LoginName,
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
                            UserAgentKey = x.UserAgentKey,
                            FP_DeviceMappingID = x.FP_DeviceMappingID,
                            Attributes = transformedAttributes
                        };
                    });

                    return Ok(transformedTrans.ToList());
                }

                return Ok(new List<TransactionByLoginNameResultModel>());
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Transaction By Login Name exception: " + ex.Message);
                return Ok(new List<TransactionByLoginNameResultModel>());
            }
        }

        private static Dictionary<string, string> DeserializeToDictionary(string attributesJson)
        {
            if (string.IsNullOrEmpty(attributesJson))
            {
                return new Dictionary<string, string>();
            }

            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(attributesJson);
            if (dict == null)
            {
                return new Dictionary<string, string>();
            }

            return dict.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value == null ? string.Empty : kvp.Value.ToString()
            );
        }

        private static FPFingerprintAttributeModel TransformFPFingerPrintInfo(Dictionary<string, string> attributesDictionary)
        {
            return JsonConvert.DeserializeObject<FPFingerprintAttributeModel>(
                       JsonConvert.SerializeObject(attributesDictionary)
                   ) ?? new FPFingerprintAttributeModel();
        }
    }
}