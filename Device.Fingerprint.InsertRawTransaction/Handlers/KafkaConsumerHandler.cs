using Confluent.Kafka;
using Device.FingerPrint.Kafka;
using Device.FingerPrint.Kafka.Ultilities;
using Fps.Device.FingerPrint.Features.Transaction.Services;
using FPS.Device.FingerPrint.Model;
using Microsoft.Extensions.Hosting;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawTransaction.Handlers
{
    public class KafkaConsumerHandler : IHostedService
    {
        private readonly ITransactionService transactionService;
        private readonly IAppSettings appSettings;
        private readonly ISentryClient sentryClient;

        public KafkaConsumerHandler(ITransactionService transactionService, IAppSettings appSettings, ISentryClient sentryClient)
        {
            this.transactionService = transactionService;
            this.appSettings = appSettings;
            this.sentryClient = sentryClient;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
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
                    List<TransactionModel> listTransaction = new List<TransactionModel>();
                    while (true)
                    {
                        var consumer = builder.Consume(cancelToken.Token);
                        var value = consumer.Message.Value;
                        TransactionModel transactionModel = SerializationHelper.DeserializeJson<TransactionModel>(value);
                        count++;
                        listTransaction.Add(transactionModel);
                        if (count == 5)
                        {
                            transactionService.InsertListTransaction(listTransaction);
                            count = 0;
                            listTransaction = new List<TransactionModel>();
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