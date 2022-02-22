using Device.FingerPrint.Features.Offset.Services;
using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.ServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using System;
using System.Linq;

namespace Device.FingerPrint.ServiceAPI.Controllers
{
    [ApiController]
    [Route("api/job")]
    public class JobController : ControllerBase
    {
        private readonly IAppSettings appSettings;
        private readonly ISentryClient sentryClient;
        private readonly IDeviceIdentifyServices deviceIdentifyServices;
        private readonly IRawTransactionService rawTransactionServices;
        private readonly IOffsetService offsetService;
        private readonly IUpdateTransactionServices updateTransactionServices;

        public JobController(IDeviceIdentifyServices deviceIdentifyServices
                            , IRawTransactionService rawTransactionServices
                            , IUpdateTransactionServices updateTransactionServices
                            , IOffsetService offsetService
                            , IAppSettings appSettings
                            , ISentryClient sentryClient)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.rawTransactionServices = rawTransactionServices;
            this.updateTransactionServices = updateTransactionServices;
            this.offsetService = offsetService;
            this.appSettings = appSettings;
            this.sentryClient = sentryClient;
        }

        [HttpPost("deviceidentify")]
        public bool DeviceIdentify([FromForm] int batchSize)
        {
            try
            {
                this.deviceIdentifyServices.DeviceIdentify(batchSize);

                return true;
            }
            catch(Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("mergetransaction")]
        public bool MergeTransaction([FromForm] int batchSize)
        {
            try
            {
                this.deviceIdentifyServices.MergeTransaction(batchSize);                

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("updatetransaction")]
        public bool UpdateTransaction([FromForm] int batchSize)
        {
            try
            {
                var offsetName = "RawTransaction";
                var retry = appSettings.RawTransactionRetry;

                // GetOffset
                var offsetRawTrans = offsetService.GetOffset(offsetName);
                var offset = offsetRawTrans.First();

                // GetRawTrans
                RawTransactionConfigModel config = new()
                {
                    Size = batchSize,
                    Offset = offset.OffsetValue,
                    Retry = retry
                };
                var rawTrans = rawTransactionServices.GetRawTrans(config);

                if (rawTrans != null && rawTrans.Any())
                {
                    // UpdateOffset
                    offsetService.UpdateOffsetValue(offsetName, rawTrans.First().Id);

                    // UpdateTransaction
                    updateTransactionServices.UpdateRawTransaction(rawTrans, batchSize, retry);
                }

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("updatetransactionN")]
        public bool UpdateTransactionN([FromForm] int batchSize)
        {
            try
            {
                var offsetName = "RawTransactionN";

                // GetOffset
                var offsetRawTrans = offsetService.GetOffset(offsetName);
                var offset = offsetRawTrans.First();

                // GetRawTrans
                RawTransactionConfigModel config = new()
                {
                    Size = batchSize,
                    Offset = offset.OffsetValue
                };

                var rawTrans = rawTransactionServices.GetRawTransN(config);
                if (rawTrans != null && rawTrans.Any())
                {
                    // UpdateOffset
                    offsetService.UpdateOffsetValue(offsetName, rawTrans.First().Id);

                    // UpdateTransaction
                    updateTransactionServices.UpdateRawTransactionN(rawTrans, batchSize);
                }

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }
    }
}
