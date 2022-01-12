using Confluent.Kafka;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using Fps.Device.FingerPrint.Features.FingerPrint.Services;
using KafkaConsumerCore;
using KafkaConsumerCore.Ultilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaConsumerFingerprint.Handlers
{
    public class KafkaConsumerHandler : IHostedService
    {
        private readonly ILogger<KafkaConsumerHandler> _logger;
        private readonly IFingerPrintService fingerPrintService;
        private readonly IAppSettings appSettings;
        public KafkaConsumerHandler(ILogger<KafkaConsumerHandler> logger, IFingerPrintService fingerPrintService, IAppSettings appSettings)
        {
            _logger = logger;
            this.fingerPrintService = fingerPrintService;
            this.appSettings = appSettings;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fingerprint worker start at: {time}", DateTimeOffset.Now);
            var conf = new ConsumerConfig
            {
                GroupId = appSettings.ConsumerGroupName,
                BootstrapServers = appSettings.BootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest
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
                        if (count == 1)
                        {
                            fingerPrintService.InsertListFingerPrint(listFingerPrint);
                            count = 0;
                            listFingerPrint = new List<FingerPrintEntity>();
                        }

                        Console.WriteLine($"Message: {value} received from {consumer.TopicPartitionOffset}");
                    }
                }
                catch (Exception e)
                {
                    builder.Close();
                    string errorMessage = e.ToString();
                    Console.WriteLine($"Error Message: {errorMessage}");
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
