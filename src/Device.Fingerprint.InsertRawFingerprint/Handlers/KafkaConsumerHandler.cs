using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Kafka.Handlers;
using Device.FingerPrint.Model;
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
            var callback = new CallBack<FingerPrintModel>(x => fingerPrintService.InsertListFingerPrint(x));
            requestTimeConsumer.StartConsumerLoop<FingerPrintModel>(cancellationToken, callback);
        }
    }
}
