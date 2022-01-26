using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Kafka.Handlers;
using Device.FingerPrint.Model;
using Microsoft.Extensions.Hosting;
using System.Linq;
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
            var callback = new CallBack<TransactionModel>(x =>
            {
                var result = x.Select(a => TranformFingerPrint(a));

                transactionService.InsertListTransaction(result);
            });

            requestTimeConsumer.StartConsumerLoop(cancellationToken, callback);
        }

        private TransactionEntity TranformFingerPrint(TransactionModel model)
        {
            return new TransactionEntity
            {
                LoginName = model.LoginName,
                ClientIp = model.ClientIp,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                TransKey = model.TransKey,
                TransTime = model.TransTime,
                Url = model.Url,
                UserAgent = model.UserAgent,
                CustId = model.CustId,
                SubAccId = model.SubAccId                
            };
        }
    }
}