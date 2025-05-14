using Device.Fingerprint.InternalAPI.Model;
using Device.Fingerprint.InternalAPI.Services;
using Device.FingerPrint.Features.FingerPrint.Entities;
using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Device.Fingerprint.InternalAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class FingerPrintController(IFingerPrintServices fingerPrintServices, ILogstashFPFingerprintService logstashFPFingerprintService) : ControllerBase
    {
        private readonly IFingerPrintServices fingerPrintServices = fingerPrintServices;

        [HttpGet(template: "GetFingerPrint/{fromDate}/{toDate}/{batchSize}/{lastFingerPrintID}")]
        [EnableRateLimiting("fixed")]
        public ActionResult<FPFingerPrintTransform> GetFingerPrint(DateTime fromDate, DateTime toDate, int batchSize, ulong lastFingerPrintID)
        {
            try
            {
                var validationResponse = ValidateRequestParameters(fromDate, toDate, batchSize, lastFingerPrintID);
                if (validationResponse != null)
                {
                    return Ok(validationResponse);
                }

                var fpFingerPrints = fingerPrintServices.GetFPFingerPrint(fromDate, toDate, batchSize, lastFingerPrintID);

                if (fpFingerPrints == null || !fpFingerPrints.Any())
                {
                    return Ok(new FPFingerPrintResponse
                    {
                        StatusCode = System.Net.HttpStatusCode.NotFound,
                        ErrorMessage = "No Fingerprint Found!",
                        FPFingerPrint = Enumerable.Empty<FPFingerPrintTransform>()
                    });
                }

                var fPTransformedList = TransformFingerprintsParallel(fpFingerPrints);

                return Ok(new FPFingerPrintResponse
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ErrorMessage = string.Empty,
                    FPFingerPrint = fPTransformedList
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
        [EnableRateLimiting("fixed")]
        public ActionResult<FPFingerPrintTransform> PushFingerprintToLogstash([FromForm] DateTime fromDate, [FromForm] DateTime toDate, [FromForm] int batchSize, [FromForm] ulong lastFingerPrintID)
        {
            try
            {
                var validationResponse = ValidateRequestParameters(fromDate, toDate, batchSize, lastFingerPrintID);

                if (validationResponse != null)
                {
                    return Ok(validationResponse);
                }

                var fpFingerPrints = fingerPrintServices.GetFPFingerPrint(fromDate, toDate, batchSize, lastFingerPrintID);

                if (fpFingerPrints == null || !fpFingerPrints.Any())
                {
                    return Ok(CreateSuccessResponse());
                }

                var fPTransformedList = TransformFingerprintsParallel(fpFingerPrints);

                var maxFingerprintId = logstashFPFingerprintService.PushLogAsync(fPTransformedList);

                return Ok(CreateSuccessResponse(maxFingerprintId));
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

        private static FPFingerPrintTransform TransformFPFingerprintAttributes(FPFingerPrintEntity fingerprint)
        {
            var attributesDictionary = DeserializeToDictionary(fingerprint.Attribute);

            var transformedAttributes = TransformFPFingerPrintInfo(attributesDictionary);

            return new FPFingerPrintTransform
            {
                ID = fingerprint.ID,
                Code = fingerprint.Code,
                Attribute = transformedAttributes,
                InsertedTime = fingerprint.InsertedTime
            };
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

        private FPFingerPrintResponse CreateSuccessResponse(ulong? maxFingerprintId = 0)
        {
            return new FPFingerPrintResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                ErrorMessage = string.Empty,
                MaxFingerprintID = maxFingerprintId
            };
        }

        private static ConcurrentBag<FPFingerPrintTransform> TransformFingerprintsParallel(IEnumerable<FPFingerPrintEntity> fingerprints)
        {
            var transformedList = new ConcurrentBag<FPFingerPrintTransform>();

            Parallel.ForEach(fingerprints, fingerprint =>
            {
                if (fingerprint != null)
                {
                    var transformed = TransformFPFingerprintAttributes(fingerprint);
                    if (transformed != null)
                    {
                        transformedList.Add(transformed);
                    }
                }
            });

            return transformedList;
        }

    }
}
