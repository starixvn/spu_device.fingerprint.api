namespace Device.FingerPrint.RawTransMonitoring.QueueConsumer.Handlers
{
    using MassTransit;
    using System.Threading.Tasks;
    using System;
    using Fanex.Logging;
    using Fanex.Logging.Extensions;
    using Device.FingerPrint.Model.TransactionMonitoring;
    using Device.FingerPrint.RawTransMonitoring.QueueConsumer.Services;
    using System.Linq;

    public class Consumer : IConsumer<Batch<TransactionMonitoringModel>>
    {
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;
        private readonly ITransactionMonitoringService transactionMonitoringService;


        public Consumer(ILogger logger, IAppSettings appSettings, ITransactionMonitoringService transactionMonitoringService)
        {
            this.logger = logger;
            this.appSettings = appSettings;
            this.transactionMonitoringService = transactionMonitoringService;

        }

        public async Task Consume(ConsumeContext<Batch<TransactionMonitoringModel>> context)
        {
            try
            {
                if (appSettings.IsConsumer)
                {
                    logger.Info("Start Consumer Raw Trans Monitoring!!!");
                    await transactionMonitoringService.SumRawTransactionAsync(context.Message.Select(x => x.Message));
                }
            }
            catch (Exception ex)
            {
                var logMessage = new LogMessage(ex.Message)
                    .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex)).ToString();

                logger.Error(logMessage);

                throw;
            }
        }
    }
}
