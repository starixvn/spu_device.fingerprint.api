using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Kafka.Handlers;
using Device.FingerPrint.Model;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawTransaction.Handlers
{
    public class KafkaConsumerHandler : BackgroundService
    {
        private readonly ITransactionService transactionService;
        private readonly IRequestTimeConsumer requestTimeConsumer;

        public KafkaConsumerHandler(ITransactionService transactionService, IRequestTimeConsumer requestTimeConsumer)
        {
            this.transactionService = transactionService;
            this.requestTimeConsumer = requestTimeConsumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new Thread(() => StartConsumerLoop(stoppingToken)).Start();

            return Task.CompletedTask;
        }
        private void StartConsumerLoop(CancellationToken cancellationToken)
        {
            var callback = new CallBack<TransactionModel>(x => transactionService.InsertListTransaction(x));
            requestTimeConsumer.StartConsumerLoop(cancellationToken, callback);
        }
    }
}