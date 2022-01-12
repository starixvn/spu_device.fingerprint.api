using Confluent.Kafka;
using Fps.Device.FingerPrint.Features.Transaction.Services;
using FPS.Device.FingerPrint.Model;
using KafkaConsumerCore.Ultilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaConsumerCore.Handlers
{
    public class KafkaConsumerHandlerCore : IHostedService
    {
        private readonly ILogger<KafkaConsumerHandlerCore> _logger;
        private readonly ITransactionService transactionService;
        private readonly IAppSettings appSettings;
        public KafkaConsumerHandlerCore(ILogger<KafkaConsumerHandlerCore> logger, ITransactionService transactionService, IAppSettings appSettings)
        {
            _logger = logger;
            this.transactionService = transactionService;
            this.appSettings = appSettings;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Transaction worker start at: {time}", DateTimeOffset.Now);
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
                        if (count == 3)
                        {
                            transactionService.InsertListTransaction(listTransaction);
                            count = 0;
                            listTransaction = new List<TransactionModel>();
                            _logger.LogInformation("Inserting Transaction at: {time}, count: {count}", DateTimeOffset.Now, count);
                        }
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
