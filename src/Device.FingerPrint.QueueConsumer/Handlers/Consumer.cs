namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Model;
    using Device.FingerPrint.Core.Services;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Model.TransactionMonitoring;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.QueueConsumer.Services;
    using Fanex.Logging;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Logging.Extensions;
    using Fanex.Oneyed.Infrastructure.Models;
    using MassTransit;

    public class Consumer : IConsumer<Batch<MessageQueue>>
    {
        private readonly IMassTransitProducer massTransitProducer;
        private readonly IRawTransactionService rawTransactionService;
        private readonly ILogTrailClientService logTrailClientService;
        private readonly ITransactionService transactionService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        private const string defaultDestination = "Device.FingerPrint.QueueConsumer";
        private const string defaultMethod = "Consumer";

        public Consumer(
                IMassTransitProducer massTransitProducer,
                IRawTransactionService rawTransactionService,
                ILogTrailClientService logTrailClientService,
                ITransactionService transactionService,
                IAppSettings appSettings,
                ILogger logger
                )
        {
            this.massTransitProducer = massTransitProducer;
            this.rawTransactionService  = rawTransactionService;
            this.logTrailClientService = logTrailClientService;
            this.transactionService = transactionService;
            this.appSettings = appSettings;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<Batch<MessageQueue>> context)
        {
            try
            {
                logger.Info("Start Consuming!", appSettings.IsExtraSub);

                if (appSettings.GetValue<bool>("Consumer:Queue-Consume"))
                {
                    await ProcessTransaction(context.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message + "\n" + JsonSerializer.Serialize(context.Message.Select(x => x.Message)));
                throw;
            }
        }

        private async Task ProcessTransaction(Batch<MessageQueue> messages)
        {
            var transactionModels = await Task.WhenAll(messages.Select(async x =>
            {
                var transaction = rawTransactionService.TransformRawTransaction(x.Message);
                await rawTransactionService.GetTransactionAttribute(transaction);
                return transaction;
            }));

            var isExtraSub = appSettings.IsExtraSub;

            rawTransactionService.TransformTransactionByIps(transactionModels, isExtraSub);

            await transactionService.InsertRawTransaction(transactionModels, isExtraSub);

            _ = InsertLogTrail(transactionModels);

            _ = HandleClearRedis(transactionModels);

            if (!isExtraSub)
            {
                _= PushTransactionToMonitorAsync(transactionModels);
            }
        }

        private async Task HandleClearRedis(IEnumerable<RawTransactionModel> transactions)
        {
            try
            {
                await rawTransactionService.ClearRedisAsync(transactions);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private async Task InsertLogTrail(IEnumerable<RawTransactionModel> transactions)
        {
            try
            {
                var logEvents = transactions.Select(x => new LogEventInfo
                {
                    EventDate = DateTime.Now.Date,
                    SubscriberID = x.SubscriberId,
                    SystemName = appSettings.SystemName,
                    Source = "test source url",
                    Destination = defaultDestination,
                    Method = defaultMethod
                });

                await logTrailClientService.PostMultipleAsync(logEvents);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private async Task PushTransactionToMonitorAsync(IEnumerable<RawTransactionModel> transactions)
        {
            if (appSettings.GetValue<bool>("RabbitMQ:Monitor:PushTrans-To-Monitor"))
            {
                var groupTrans = transactions.Select(x => { x.TransTime = Convert.ToDateTime(x.TransTime.ToString("MM/dd/yyyy")); return x; })
                                    .GroupBy(x => new { x.SubscriberId, x.SubscriberName, x.TransTime })
                                    .Select(x => new TransactionMonitoringModel
                                    {
                                        SubscriberID = x.Key.SubscriberId,
                                        SubscriberName = x.Key.SubscriberName,
                                        TransDate = x.Key.TransTime,
                                        TransTotal = x.Count()
                                    });

                if (groupTrans != null && groupTrans.Any())
                {
                    await massTransitProducer.SendBatchAsync(groupTrans.ToList(), appSettings.GetValue<string>("RabbitMQ:Monitor:Name"));
                }
            }
        }

        private void LogError(Exception ex)
        {
            var logMessage = new LogMessage(ex.Message)
                   .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                   .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                   .ToString();

            logger.Error(logMessage);
        }
    }
}
