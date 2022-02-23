using Device.FingerPrint.Features.Offset.Services;
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
        private readonly ISentryClient sentryClient;
        private readonly IDeviceIdentifyServices deviceIdentifyServices;
        private readonly IOffsetService offsetService;

        public JobController(IDeviceIdentifyServices deviceIdentifyServices
                            , IOffsetService offsetService
                            , ISentryClient sentryClient)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.offsetService = offsetService;
            this.sentryClient = sentryClient;
        }

        [HttpPost("deviceidentify")]
        public bool DeviceIdentify([FromForm] int batchSize)
        {
            try
            {
                this.deviceIdentifyServices.ServiceDeviceIdentify(batchSize);

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
                this.deviceIdentifyServices.ServiceUpdateTransaction(batchSize);

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
                this.deviceIdentifyServices.ServiceUpdateTransactionN(batchSize);

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
