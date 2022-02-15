using Device.FingerPrint.ServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sentry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [HttpPost("deviceidentify/{batchSize}")]
        public bool DeviceIdentify(int batchSize)
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

        [HttpPost("mergetransaction/{batchSize}")]
        public bool MergeTransaction(int batchSize)
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

        [HttpPost("updateTransaction/{batchSize}")]
        public bool UpdateTransaction(int batchSize)
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
    }
}
