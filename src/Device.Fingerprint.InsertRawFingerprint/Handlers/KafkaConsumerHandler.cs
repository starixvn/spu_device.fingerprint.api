using Device.FingerPrint.Kafka;
using Device.FingerPrint.Kafka.Handlers;
using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Features.FingerPrint.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawFingerprint.Handlers
{
    public class KafkaConsumerHandler : BackgroundService
    {
        private readonly IFingerPrintService fingerPrintService;
        private readonly IRequestTimeConsumer requestTimeConsumer;

        public KafkaConsumerHandler(IFingerPrintService fingerPrintService, IRequestTimeConsumer requestTimeConsumer)
        {
            this.fingerPrintService = fingerPrintService;
            this.requestTimeConsumer = requestTimeConsumer;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new Thread(() => StartConsumerLoop(stoppingToken)).Start();

            return Task.CompletedTask;
        }
        private void StartConsumerLoop(CancellationToken cancellationToken)
        {
            var callback = new CallBack<FingerPrintEntity>(x => fingerPrintService.InsertListFingerPrint(x));
            requestTimeConsumer.StartConsumerLoop<FingerPrintEntity>(cancellationToken, callback);
        }
    }
}
