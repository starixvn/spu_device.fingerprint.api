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
        private readonly IMonitorJobServices monitorJobServices;

        public JobController(IDeviceIdentifyJobServices deviceIdentifyServices
                            , ISentryClient sentryClient
                            , ITransactionJobServices transactionServices
                            , IMonitorJobServices monitorJobServices)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.sentryClient = sentryClient;
            this.transactionServices = transactionServices;
            this.monitorJobServices = monitorJobServices;
        }

        [HttpPost("deviceidentify")]
        public bool DeviceIdentify([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                deviceIdentifyServices.DeviceIdentify(batchSize, numberOfBatch);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("mergetransaction")]
        public bool MergeTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                transactionServices.MergeTrans(batchSize, numberOfBatch);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("forcemergetransaction")]
        public bool ForceMergeTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                transactionServices.ForceMergeTrans(batchSize, numberOfBatch);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("updatetransaction")]
        public bool UpdateTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            try
            {
                transactionServices.UpdateTransaction(batchSize, numberOfBatch, retry);

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

        [HttpPost("monitordailysum")]
        public bool MonitorDailySum()
        {
            try
            {
                monitorJobServices.DailyReport();

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("monthlysum")]
        public bool MonthlySum()
        {
            try
            {
                monitorJobServices.MonthlySum();

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
