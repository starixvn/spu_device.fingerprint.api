namespace Device.FingerPrint.ServiceAPI.Controllers
{
    using Device.FingerPrint.Core.Model;
    using Device.FingerPrint.Model.TransactionMonitoring;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.ServiceAPI.Services;
    using Fanex.Logging;
    using Fanex.LogTrail.Service;
    using Microsoft.AspNetCore.Mvc;
    using Serilog.Events;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;

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
        private readonly ILogger logger;
        private readonly IMassTransitProducer massTransitProducer;
        private readonly IAppSettings appSettings;
        private readonly ILogEvent logEvent;

        public JobController(IDeviceIdentifyJobServices deviceIdentifyServices
                            , ITransactionJobServices transactionServices
                            , IMonitorJobServices monitorJobServices
                            , ISummaryJobServices summaryJobServices
                            , IFPDeviceJobService fpdeviceJobService
                            , IFPTransactionJobService fPTransactionJobService
                            , IArchiveJobServices archiveJobServices
                            , IEvolutionJobServices evolutionJobServices
                            , ILogger logger
                            , IMassTransitProducer massTransitProducer
                            , IAppSettings appSettings
                            , ILogEvent logEvent
                            )
        {
            this.deviceIdentifyServices = deviceIdentifyServices;
            this.transactionServices = transactionServices;
            this.monitorJobServices = monitorJobServices;
            this.summaryJobServices = summaryJobServices;
            this.fpdeviceJobService = fpdeviceJobService;
            this.fPTransactionJobService = fPTransactionJobService;
            this.archiveJobServices = archiveJobServices;
            this.evolutionJobServices = evolutionJobServices;
            this.logger = logger;
            this.massTransitProducer = massTransitProducer;
            this.appSettings = appSettings;
            this.logEvent = logEvent;
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

        [HttpPost("pushLogTrailEvent")]
        public async Task PushLogTrailEventAsync([FromBody] LogEventModel[] body)
        {
            try
            {
                if (body.Length > 0)
                {
                    var logEvents = body.Select(x => JsonSerializer.Deserialize<LogEventInfo>(x.RenderedMessage));
                    _ = massTransitProducer.SendBatchAsync(logEvents, appSettings.GetValue<string>("RabbitMQ:LogTrail:QueueName"));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        [HttpGet("hc")]
        public void HealthCheck()
        {
            logger.Info("I'm good!");
        }

        [HttpGet("testLogEvent")]
        public async Task testLogEvent()
        {
            var tests = new List<LogEventInfo>();
            for (int i = 0; i <= 10; i++)
            {
                var test = new LogEventInfo
                {
                    EventDate = DateTime.Now,
                    SubscriberID = 2,
                    SystemName = "Test SystemName",
                    Source = "Test Source",
                    Destination = "Test Destination",
                    Method = "Test Method",
                    Total = 0
                };
                tests.Add(test);
            }

            _ = Task.WhenAll(tests.Select(async x =>
            {
                await logEvent.WriteEventInfoAsync(x);
            }));
        }

        [HttpGet("testMassTrans")]
        public async Task testMassTrans()
        {
            var test = new TransactionMonitoringModel
            {
                TransDate = DateTime.Now,
                SubscriberID = 1,
                SubscriberName = "Test123",
                TransTotal = 10,
            };

            await massTransitProducer.SendAsync(test, "queue:dcsTransQueue");
        }
    }
}
