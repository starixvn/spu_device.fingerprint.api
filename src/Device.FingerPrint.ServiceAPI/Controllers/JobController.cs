using Device.FingerPrint.ServiceAPI.Services;
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
        private readonly IDeviceIdentifyJobServices deviceIdentifyServices;
        private readonly ITransactionJobServices transactionServices;

        public JobController(IDeviceIdentifyJobServices deviceIdentifyServices
                            , ISentryClient sentryClient
                            , ITransactionJobServices transactionServices)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.sentryClient = sentryClient;
            this.transactionServices = transactionServices;
        }

        [HttpPost("deviceidentify")]
        public bool DeviceIdentify([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                this.deviceIdentifyServices.DeviceIdentify(batchSize, numberOfBatch);

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
                transactionServices.MergeTrans(batchSize);                

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("forcemergetransaction")]
        public bool ForceMergeTransaction([FromForm] int batchSize)
        {
            try
            {
                transactionServices.ForceMergeTrans(batchSize);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("updatetransaction")]
        public bool UpdateTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                transactionServices.UpdateTransaction(batchSize, numberOfBatch);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("updatetransactionN")]
        public bool UpdateTransactionN([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                transactionServices.UpdateTransactionN(batchSize, numberOfBatch);

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
