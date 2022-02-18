using Device.FingerPrint.ServiceAPI.Services;
using Fanex.Logging;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using System;

namespace Device.FingerPrint.ServiceAPI.Controllers
{
    [ApiController]
    [Route("api/job")]
    public class JobController : ControllerBase
    {
        private readonly ISentryClient sentryClient;
        private readonly IDeviceIdentifyServices deviceIdentifyServices;
        public JobController(IDeviceIdentifyServices deviceIdentifyServices, ISentryClient sentryClient)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
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
                this.deviceIdentifyServices.UpdateTransaction(batchSize);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("updatetransactionN")]
        public bool UpdateTransactionN()
        {
            try
            {
                this.deviceIdentifyServices.UpdateTransactionN();

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
