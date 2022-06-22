using Device.FingerPrint.ServiceAPI.Services;
using Fanex.Logging;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Controllers
{
    [ApiController]
    [Route("api/job")]
    public class JobController : ControllerBase
    {
        private readonly IDeviceIdentifyJobServices deviceIdentifyServices;
        private readonly ITransactionJobServices transactionServices;
        private readonly IMonitorJobServices monitorJobServices;
        private readonly ISummaryJobServices summaryJobServices;
        private readonly IRawUserAgentJobServices rawUserAgentJobServices;
        private readonly IFPDeviceJobService fpdeviceJobService;
        private readonly IFPTransactionJobService fPTransactionJobService;
        private readonly IArchiveJobServices archiveJobServices;
        private readonly IBrowserInfoJobServices browserInfoJobServices;

        public JobController(IDeviceIdentifyJobServices deviceIdentifyServices
                            , ITransactionJobServices transactionServices
                            , IMonitorJobServices monitorJobServices
                            , ISummaryJobServices summaryJobServices
                            , IRawUserAgentJobServices rawUserAgentJobServices
                            , IFPDeviceJobService fpdeviceJobService
                            , IFPTransactionJobService fPTransactionJobService
                            , IArchiveJobServices archiveJobServices)
                            , IBrowserInfoJobServices browserInfoJobServices)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.transactionServices = transactionServices;
            this.monitorJobServices = monitorJobServices;
            this.summaryJobServices = summaryJobServices;
            this.rawUserAgentJobServices = rawUserAgentJobServices;
            this.fpdeviceJobService = fpdeviceJobService;
            this.fPTransactionJobService = fPTransactionJobService;
            this.browserInfoJobServices = browserInfoJobServices;
            this.archiveJobServices = archiveJobServices;
        }

        [HttpPost("deviceidentify")]
        public Task<bool> DeviceIdentify([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                deviceIdentifyServices.DeviceIdentify(batchSize, numberOfBatch);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("mergetransaction")]
        public Task<bool> MergeTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            try
            {
                transactionServices.MergeTrans(batchSize, numberOfBatch, retry);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("forcemergetransaction")]
        public Task<bool> ForceMergeTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            try
            {
                transactionServices.ForceMergeTrans(batchSize, numberOfBatch, retry);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("updatetransaction")]
        public Task<bool> UpdateTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            try
            {
                transactionServices.UpdateTransaction(batchSize, numberOfBatch, retry);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("updatetransactionretry")]
        public Task<bool> UpdateTransactionRetry([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            try
            {
                transactionServices.UpdateTransactionRetry(batchSize, numberOfBatch, retry);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("updatetransactionN")]
        public Task<bool> UpdateTransactionN([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            try
            {
                transactionServices.UpdateTransactionN(batchSize, numberOfBatch, retry);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("transformfpdevice")]
        public Task<bool> TransformFPDevice([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                fpdeviceJobService.Execute(batchSize, numberOfBatch);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("transformfptransaction")]
        public Task<bool> TransformFPTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                fPTransactionJobService.Execute(batchSize, numberOfBatch);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("summaryservertransactiondaily")]
        public Task<bool> SummaryServerTransactionDaily([FromForm] int batchSize)
        {
            try
            {
                summaryJobServices.SummaryServerTransactionDaily(batchSize);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("summaryclienttransactiondaily")]
        public Task<bool> SummaryClientTransactionDaily([FromForm] int batchSize)
        {
            try
            {
                summaryJobServices.SummaryClientTransactionDaily(batchSize);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("summarytransactiondaily")]
        public Task<bool> SummaryTransactionDaily([FromForm] int batchSize)
        {
            try
            {
                summaryJobServices.SummaryTransactionDaily(batchSize);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("monitordailyreport")]
        public Task<bool> MonitorDailyReport()
        {
            try
            {
                monitorJobServices.DailyReport();

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("monitormonthlyreport")]
        public Task<bool> MonitorMonthlyReport()
        {
            try
            {
                monitorJobServices.MonthlyReport();

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("detectbots")]
        public bool DetectBots([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            try
            {
                browserInfoJobServices.DetectBots(batchSize, numberOfBatch);

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return Task.FromResult(false);
            }
        }

        [HttpPost("updatemodifieddate")]
        public Task<bool> UpdateModifiedDate([FromForm] int batchSize)
        {
            try
            {
                archiveJobServices.UpdateModifiedDate(batchSize);

                return true;
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);

                return false;
            }
        }

    }
}
