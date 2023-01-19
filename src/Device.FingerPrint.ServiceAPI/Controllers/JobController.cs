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
        private readonly IEvolutionJobServices evolutionJobServices;

        public JobController(IDeviceIdentifyJobServices deviceIdentifyServices
                            , ITransactionJobServices transactionServices
                            , IMonitorJobServices monitorJobServices
                            , ISummaryJobServices summaryJobServices
                            , IFPDeviceJobService fpdeviceJobService
                            , IFPTransactionJobService fPTransactionJobService
                            , IArchiveJobServices archiveJobServices
                            , IEvolutionJobServices evolutionJobServices)
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.transactionServices = transactionServices;
            this.monitorJobServices = monitorJobServices;
            this.summaryJobServices = summaryJobServices;
            this.fpdeviceJobService = fpdeviceJobService;
            this.fPTransactionJobService = fPTransactionJobService;
            this.archiveJobServices = archiveJobServices;
            this.evolutionJobServices = evolutionJobServices;
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

        [HttpPost("summarytransactiondaily")]
        public async Task<bool> SummaryTransactionDaily([FromForm] int batchSize)
        {
            return await summaryJobServices.SummaryTransactionDaily(batchSize);
        }

        [HttpPost("summaryrecovertypedaily")]
        public async Task<bool> SummaryRecoverTypeDaily([FromForm] int batchSize)
        {
            return await summaryJobServices.SummaryRecoverTypeDaily(batchSize);
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

        [HttpPost("updatemodifieddate")]
        public async Task<bool> UpdateModifiedDate([FromForm] int batchSize)
        {
            return await archiveJobServices.Execute(batchSize);
        }

        [HttpPost("evolution")]
        public async Task<bool> Evolution([FromForm] int batchSize, [FromForm] int numberOfBatch)
        {
            return await evolutionJobServices.Evolution(batchSize, numberOfBatch);
        }

        [HttpPost("testSentryCaptureMessage")]
        public async Task<bool> TestSentryCaptureMessage([FromForm] string message)
        {
            return await monitorJobServices.TestSentryCaptureMessage(message);
        }

        [HttpPost("testNotificationTelegram")]
        public void TestNotificationTelegram([FromForm] string message)
        {
            monitorJobServices.TestNotificationTelegram(message);
        }
    }
}
