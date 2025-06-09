using Device.Fingerprint.InternalAPI.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.Model.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;


namespace Device.Fingerprint.InternalAPI.Controllers
{
    [Route("api/raw-trans")]
    [ApiController]
    public class RawTransactionController(ITransactionService transactionService) : ControllerBase
    {
        [HttpPost("insertRawTrans")]
        [EnableRateLimiting("fixed")]
        public ActionResult<TransactionInfoModel> InsertRawTransaction([FromBody] string rawTransJson)
        {
            try
            {
                var isExtraSub = false;

                var rawTransModels = JsonConvert.DeserializeObject<List<RawTransactionModel>>(rawTransJson);

                if (rawTransModels != null && rawTransModels.Count != 0)
                {
                    transactionService.InsertRawTransaction(rawTransModels, isExtraSub);

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

        [HttpPost("insertFP")]
        [EnableRateLimiting("fixed")]
        public ActionResult<TransactionInfoModel> InsertFingerprint([FromBody] IEnumerable<InsertFPFingerprintEntity> inputEntity)
        {
            try
            {
                //inputString = inputString.Replace(":NULL", ":\"NULL\"");

                //var model = JsonConvert.DeserializeObject<IEnumerable<InsertFPFingerprintEntity>>(inputString);
                var fingerPrintJson = JsonConvert.SerializeObject(inputEntity);

                transactionService.InsertFPFingerPrint(fingerPrintJson, isTransOfDBExtra: false);

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

        [HttpPost("insertUA")]
        [EnableRateLimiting("fixed")]
        public ActionResult<TransactionInfoModel> InsertUserAgent([FromBody] IEnumerable<UserAgentInfoModel> inputEntity)
        {
            try
            {
                var userAgentJson = JsonConvert.SerializeObject(inputEntity);

                transactionService.InsertFPUserAgent(userAgentJson, isTransOfDBExtra: false);

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

    }
}
