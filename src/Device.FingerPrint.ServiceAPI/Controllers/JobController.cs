using Device.FingerPrint.Features.SummaryTransaction.Services;
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
        private readonly ISummaryJobServices summaryJobServices;
        private readonly IRawUserAgentJobServices rawUserAgentJobServices;
        private readonly IFPDeviceJobService fpdeviceJobService;
        private readonly IFPTransactionJobService fPTransactionJobService;

        public JobController(IDeviceIdentifyJobServices deviceIdentifyServices
                            , ISentryClient sentryClient
                            , ITransactionJobServices transactionServices
                            , IMonitorJobServices monitorJobServices
                            , ISummaryJobServices summaryJobServices
                            , IRawUserAgentJobServices rawUserAgentJobServices
                            , IFPDeviceJobService fpdeviceJobService
                            , IFPTransactionJobService fPTransactionJobService)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.sentryClient = sentryClient;
            this.transactionServices = transactionServices;
            this.monitorJobServices = monitorJobServices;
            this.summaryJobServices = summaryJobServices;
            this.rawUserAgentJobServices = rawUserAgentJobServices;
            this.fpdeviceJobService = fpdeviceJobService;
            this.fPTransactionJobService = fPTransactionJobService;
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

        [HttpPost("transformfpdevice")]
        public bool TransformFPDevice([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                fpdeviceJobService.Execute(batchSize, numberOfBatch);
                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("transformfptransaction")]
        public bool TransformFPTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                fPTransactionJobService.Execute(batchSize, numberOfBatch);
                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("summaryservertransactiondaily")]
        public bool SummaryServerTransactionDaily([FromForm] int batchSize)
        {
            try
            {
                summaryJobServices.SummaryServerTransactionDaily(batchSize);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("summaryclienttransactiondaily")]
        public bool SummaryClientTransactionDaily([FromForm] int batchSize)
        {
            try
            {
                summaryJobServices.SummaryClientTransactionDaily(batchSize);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("summarytransactiondaily")]
        public bool SummaryTransactionDaily([FromForm] int batchSize)
        {
            try
            {
                summaryJobServices.SummaryTransactionDaily(batchSize);

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("summarytransactionmonthly")]
        public bool SummaryTransactionMonthly()
        {
            try
            {
                summaryJobServices.SummaryTransactionMonthly();

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("monitordailyreport")]
        public bool MonitorDailyReport()
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

        [HttpPost("monitormonthlyreport")]
        public bool MonitorMonthlyReport()
        {
            try
            {
                monitorJobServices.MonthlyReport();

                return true;
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);

                return false;
            }
        }

        [HttpPost("devicedetector")]
        public bool DeviceDetector([FromForm] int batchSize)
        {
            try
            {
                rawUserAgentJobServices.UpdateRawUserAgent(batchSize);

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
