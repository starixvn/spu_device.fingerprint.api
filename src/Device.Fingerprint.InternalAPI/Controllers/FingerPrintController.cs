using Device.Fingerprint.InternalAPI.Model;
using Device.Fingerprint.InternalAPI.Services;
using Device.FingerPrint.Core;
using Device.FingerPrint.Features.FingerPrint.Entities;
using Device.FingerPrint.Features.FingerPrint.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Device.Fingerprint.InternalAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class FingerPrintController(IAppSettings appSettings, IFingerPrintServices fingerPrintServices, ILogstashFPFingerprintService logstashFPFingerprintService) : ControllerBase
    {
        private readonly IFingerPrintServices fingerPrintServices = fingerPrintServices;

        [HttpGet(template: "GetFingerPrint/{fromDate}/{toDate}/{batchSize}/{lastFingerPrintID}")]
        [AllowAnonymous]
        public ActionResult<FPFingerPrintEntity> GetFingerPrint(DateTime fromDate, DateTime toDate, int batchSize, ulong lastFingerPrintID)
        {
            try
            {
                var validationResponse = ValidateRequestParameters(fromDate, toDate, batchSize, lastFingerPrintID);
                if (validationResponse != null)
                {
                    return Ok(validationResponse);
                }

                var fpFingerPrint = fingerPrintServices.GetFPFingerPrint(fromDate, toDate, batchSize, lastFingerPrintID);

                return Ok(new FPFingerPrintResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    FPFingerPrint = fpFingerPrint
                });
            }
            catch (Exception ex)
            {
                return Ok(new FPFingerPrintResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = ex.Message
                });
            }
        }

        [HttpPost("PushFingerprintToLogstash")]
        [AllowAnonymous]
        public ActionResult<FPFingerPrintEntity> PushFingerprintToLogstash([FromForm] DateTime fromDate, [FromForm] DateTime toDate, [FromForm] int batchSize, [FromForm] ulong lastFingerPrintID)
        {
            try
            {
                var validationResponse = ValidateRequestParameters(fromDate, toDate, batchSize, lastFingerPrintID);
                if (validationResponse != null)
                {
                    return Ok(validationResponse);
                }

                var fingerprints = fingerPrintServices.GetFPFingerPrint(fromDate, toDate, batchSize, lastFingerPrintID);

                if (fingerprints == null || !fingerprints.Any())
                {
                    return Ok(CreateSuccessResponse());
                }

                var maxFingerprintId = logstashFPFingerprintService.PushLogAsync(fingerprints);

                return Ok(CreateSuccessResponse(maxFingerprintId.ToString()));
            }
            catch (Exception ex)
            {
                return Ok(new FPFingerPrintResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = ex.Message
                });
            }
        }

        private static FPFingerPrintResponse? ValidateRequestParameters(DateTime fromDate, DateTime toDate, int batchSize, ulong lastFingerPrintID)
        {
            if (fromDate > toDate)
            {
                return new FPFingerPrintResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = "FromDate cannot be greater than ToDate!"
                };
            }

            if (lastFingerPrintID < 0)
            {
                return new FPFingerPrintResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = "LastFingerPrintID cannot be less than 0!"
                };
            }

            if (batchSize > 10000)
            {
                return new FPFingerPrintResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMessage = "BatchSize cannot be greater than 10000!"
                };
            }

            return null;
        }

        private FPFingerPrintResponse CreateSuccessResponse(string? maxFingerprintId = null)
        {
            return new FPFingerPrintResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                MaxFingerprintID = maxFingerprintId
            };
        }
    }
}
