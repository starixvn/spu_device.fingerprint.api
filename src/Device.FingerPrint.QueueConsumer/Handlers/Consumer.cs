namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Model;
    using Device.FingerPrint.Core.Services;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Model.TransactionMonitoring;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.QueueConsumer.Services;
    using Fanex.Logging;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.Database;
    using Fanex.Oneyed.Infrastructure.Models;
    using MassTransit;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
            this.rawTransactionService = rawTransactionService;
            this.logTrailClientService = logTrailClientService;
            this.transactionService = transactionService;
            this.appSettings = appSettings;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<Batch<MessageQueue>> context)
        {
            try
            {
                logger.Info("Start Consuming!" + appSettings.IsExtraSub);

                if (appSettings.GetValue<bool>("Consumer:IsConsume"))
                {
                    await ProcessTransaction(context.Message);
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                throw;
            }
        }

        private async Task ProcessTransaction(Batch<MessageQueue> messages)
        {
            var transactionModels = await Task.WhenAll(messages.Select(async x =>
            {
                return await rawTransactionService.TransformRawTransaction(x.Message);
            }));

            rawTransactionService.TransformTransactionByIps(transactionModels);

            await transactionService.InsertRawTransaction(transactionModels, appSettings.IsExtraSub);

            AdditionalProcessingNotAwait(transactionModels);
        }

        private void AdditionalProcessingNotAwait(IEnumerable<RawTransactionModel> transactionModels)
        {
            var tasks = new List<Task>
            {
                InsertLogTrail(transactionModels),
                HandleClearRedis(transactionModels),
                PushTransactionToMonitorAsync(transactionModels)
            };

            _ = Task.WhenAll(tasks);
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
            if (appSettings.GetValue<bool>("RabbitMQ:Monitor:PushTrans-To-Monitor") && !appSettings.IsExtraSub)
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
