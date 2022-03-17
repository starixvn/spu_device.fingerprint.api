using Device.FingerPrint.Kafka;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;
using Microsoft.Extensions.Hosting;
using Sentry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawFingerprint.Handlers
{
    public class KafkaConsumer : BackgroundService
    {
        private readonly ISentryClient sentryClient;
        private readonly IAppSettings appSettings;
        private readonly IKafkaConsumer<string, FingerPrintModel> consumer;

        public KafkaConsumer(ISentryClient sentryClient, IAppSettings appSettings, IKafkaConsumer<string, FingerPrintModel> consumer)
        {
            this.sentryClient = sentryClient;
            this.appSettings = appSettings;
            this.consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await consumer.Consume(appSettings.KafkaTopic, stoppingToken);
            }
            catch (Exception ex)
            {
                this.sentryClient.CaptureException(ex);
            }
        }
    }
}
