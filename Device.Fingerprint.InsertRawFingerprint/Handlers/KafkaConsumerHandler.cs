using Confluent.Kafka;
using Device.FingerPrint.Kafka;
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
        private readonly IFingerPrintService fingerPrintService;
        private readonly IAppSettings appSettings;
        private readonly ISentryClient sentryClient;

        public KafkaConsumerHandler(IFingerPrintService fingerPrintService, IAppSettings appSettings, ISentryClient sentryClient)
        {
            this.fingerPrintService = fingerPrintService;
            this.appSettings = appSettings;
            this.sentryClient = sentryClient;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var conf = new ConsumerConfig
            {
                GroupId = appSettings.ConsumerGroupName,
                BootstrapServers = appSettings.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
            using (var builder = new ConsumerBuilder<Ignore, string>(conf).Build())
            {
                builder.Subscribe(appSettings.TopicName);
                var cancelToken = new CancellationTokenSource();
                try
                {
                    int count = 0;
                    List<FingerPrintEntity> listFingerPrint = new List<FingerPrintEntity>();
                    while (true)
                    {
                        var consumer = builder.Consume(cancelToken.Token);
                        var value = consumer.Message.Value;
                        FingerPrintEntity fingerPrintEntity = SerializationHelper.DeserializeJson<FingerPrintEntity>(value);
                        count++;
                        listFingerPrint.Add(fingerPrintEntity);
                        if (count == 10)
                        {
                            fingerPrintService.InsertListFingerPrint(listFingerPrint);
                            builder.Commit(consumer);
                            count = 0;
                            listFingerPrint = new List<FingerPrintEntity>();
                        }
                    }
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
