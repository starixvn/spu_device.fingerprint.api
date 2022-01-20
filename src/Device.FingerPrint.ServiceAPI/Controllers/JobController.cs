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
        private readonly IRecoverDeviceServices recoverDeviceServices;
        public JobController(IRecoverDeviceServices recoverDeviceServices, ISentryClient sentryClient)
        {
            this.recoverDeviceServices = recoverDeviceServices;
            this.sentryClient = sentryClient;
        }

        [HttpPost("recoverdevice/{batchSize}")]
        public bool RecoverDevice(int batchSize)
        {
            try
            {
                this.recoverDeviceServices.RecoverDevice(batchSize);

                return true;
            }
            catch(Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpGet("testing")]
        public string Test()
        {
            return "testing";
        }
    }
}
