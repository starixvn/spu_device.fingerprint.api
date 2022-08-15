using Device.FingerPrint.ServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IFPDeviceJobService fpdeviceJobService;
        private readonly IFPTransactionJobService fPTransactionJobService;
        private readonly IArchiveJobServices archiveJobServices;
        private readonly IBrowserInfoJobServices browserInfoJobServices;
        private readonly IRawUserAgentJobServices rawUserAgentJobServices;

        public JobController(IDeviceIdentifyJobServices deviceIdentifyServices
                            , ITransactionJobServices transactionServices
                            , IMonitorJobServices monitorJobServices
                            , ISummaryJobServices summaryJobServices
                            , IFPDeviceJobService fpdeviceJobService
                            , IFPTransactionJobService fPTransactionJobService
                            , IArchiveJobServices archiveJobServices
                            , IBrowserInfoJobServices browserInfoJobServices
                            , IRawUserAgentJobServices rawUserAgentJobServices)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.transactionServices = transactionServices;
            this.monitorJobServices = monitorJobServices;
            this.summaryJobServices = summaryJobServices;
            this.fpdeviceJobService = fpdeviceJobService;
            this.fPTransactionJobService = fPTransactionJobService;
            this.archiveJobServices = archiveJobServices;
            this.browserInfoJobServices = browserInfoJobServices;
            this.rawUserAgentJobServices = rawUserAgentJobServices;
        }

        [HttpPost("deviceidentify")]
        public async Task<bool> DeviceIdentify([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            return await deviceIdentifyServices.DeviceIdentify(batchSize, numberOfBatch);
        }

        [HttpPost("mergetransaction")]
        public async Task<bool> MergeTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            return await transactionServices.MergeTrans(batchSize, numberOfBatch, retry);
        }

        [HttpPost("forcemergetransaction")]
        public async Task<bool> ForceMergeTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            return await transactionServices.ForceMergeTrans(batchSize, numberOfBatch, retry);
        }

        [HttpPost("updatetransaction")]
        public async Task<bool> UpdateTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            return await transactionServices.UpdateTransaction(batchSize, numberOfBatch, retry);
        }

        [HttpPost("updatetransactionretry")]
        public async Task<bool> UpdateTransactionRetry([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            return await transactionServices.UpdateTransactionRetry(batchSize, numberOfBatch, retry);
        }

        [HttpPost("updatetransactionN")]
        public async Task<bool> UpdateTransactionN([FromForm] int batchSize, [FromForm] int numberOfBatch, [FromForm] int retry)
        {
            return await transactionServices.UpdateTransactionN(batchSize, numberOfBatch, retry);
        }

        [HttpPost("transformfpdevice")]
        public async Task<bool> TransformFPDevice([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            return await fpdeviceJobService.Execute(batchSize, numberOfBatch);
        }

        [HttpPost("transformfptransaction")]
        public async Task<bool> TransformFPTransaction([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            return await fPTransactionJobService.Execute(batchSize, numberOfBatch);
        }

        [HttpPost("summaryservertransactiondaily")]
        public async Task<bool> SummaryServerTransactionDaily([FromForm] int batchSize)
        {
            return await summaryJobServices.SummaryServerTransactionDaily(batchSize);
        }

        [HttpPost("summaryclienttransactiondaily")]
        public async Task<bool> SummaryClientTransactionDaily([FromForm] int batchSize)
        {
            return await summaryJobServices.SummaryClientTransactionDaily(batchSize);
        }

        [HttpPost("summarytransactiondaily")]
        public async Task<bool> SummaryTransactionDaily([FromForm] int batchSize)
        {
            return await summaryJobServices.SummaryTransactionDaily(batchSize);
        }

        [HttpPost("monitordailyreport")]
        public async Task<bool> MonitorDailyReport()
        {
            return await monitorJobServices.DailyReport();
        }

        [HttpPost("monitormonthlyreport")]
        public async Task<bool> MonitorMonthlyReport()
        {
            return await monitorJobServices.MonthlyReport();
        }

        [HttpPost("detectbots")]
        public async Task<bool> DetectBots([FromForm] int batchSize, [FromForm] int detectionLevel)
        {
            /* detectionLevel: 
             * 0 || 1 => Check Headless score
             * 2 => Check BrowserName & OSName
             */
            return await browserInfoJobServices.DetectBots(batchSize, detectionLevel);
        }

        [HttpPost("updatemodifieddate")]
        public async Task<bool> UpdateModifiedDate([FromForm] int batchSize)
        {
            return await archiveJobServices.Execute(batchSize);
        }

        [HttpPost("parseuseragent")]
        public async Task<bool> ParseUserAgent([FromForm] int batchSize)
        {
            return await rawUserAgentJobServices.UpdateRawUserAgent(batchSize);
        }

    }
}
