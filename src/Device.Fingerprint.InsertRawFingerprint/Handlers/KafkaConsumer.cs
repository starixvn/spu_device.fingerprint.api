using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Kafka.Handlers;
using Device.FingerPrint.Model;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Sentry;
using System;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;

namespace Device.Fingerprint.InsertRawFingerprint.Handlers
{
    public class KafkaConsumer : BackgroundService
    {        
        private readonly ISentryClient sentryClient;
        private readonly IKafkaConsumer<string, FingerPrintModel> consumer;

        public KafkaConsumer(ISentryClient sentryClient, IKafkaConsumer<string, FingerPrintModel> consumer)
        {
            this.sentryClient = sentryClient;
            this.consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await consumer.Consume("fingerprint-topic", stoppingToken);
            }
            catch (Exception ex)
            {
                this.sentryClient.CaptureException(ex);
            }
        }
    }
}
