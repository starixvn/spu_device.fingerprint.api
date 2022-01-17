using Device.FingerPrint.Kafka;
using Device.FingerPrint.Kafka.Handlers;
using Fps.Device.FingerPrint.Features.Transaction.Services;
using FPS.Device.FingerPrint.Model;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawTransaction.Handlers
{
    public class KafkaConsumerHandler : IHostedService
    {
        private readonly IAppSettings appSettings;
        private readonly ITransactionService transactionService;
        private readonly IKafkaHandler kafkaHandler;

        public KafkaConsumerHandler(IAppSettings appSettings, ITransactionService transactionService, IKafkaHandler kafkaHandler)
        {
            this.transactionService = transactionService;
            this.kafkaHandler = kafkaHandler;
            this.appSettings = appSettings;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var callback = new CallBack<TransactionModel>(x => transactionService.InsertListTransaction(x));

            return kafkaHandler.StartAsync(cancellationToken, appSettings, callback);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return kafkaHandler.StopAsync(cancellationToken);
        }
    }
}