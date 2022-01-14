using Confluent.Kafka;
using Device.FingerPrint.Kafka;
using Device.FingerPrint.Kafka.Handlers;
using Device.FingerPrint.Kafka.Ultilities;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using Fps.Device.FingerPrint.Features.FingerPrint.Services;
using Microsoft.Extensions.Hosting;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawFingerprint.Handlers
{
    public class KafkaConsumerHandler : IHostedService
    {
        private readonly IAppSettings appSettings;
        private readonly IFingerPrintService fingerPrintService;
        private readonly IKafkaHandler kafkaHandler;

        public KafkaConsumerHandler(IAppSettings appSettings, IFingerPrintService fingerPrintService, IKafkaHandler kafkaHandler)
        {
            this.fingerPrintService = fingerPrintService;
            this.kafkaHandler = kafkaHandler;
            this.appSettings = appSettings;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var callback = new CallBack<FingerPrintEntity>((list) => fingerPrintService.InsertListFingerPrint(list));
            return kafkaHandler.StartAsync<FingerPrintEntity>(cancellationToken, appSettings, callback);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return kafkaHandler.StopAsync(cancellationToken);
        }
    }
}
