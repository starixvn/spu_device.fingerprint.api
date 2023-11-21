namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Model;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Model.TransactionMonitoring;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.QueueConsumer.Services;
    using Fanex.Logging;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.Database;
    using Fanex.LogTrail.Service;
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
        private readonly ITransactionService transactionService;
        private readonly IIPDBService ipdbService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;
        private readonly ILogEvent logEvent;

        private const string defaultDestination = "DCS_RawTrans";
        private const string defaultSource = "RabbitMQ";
        private const string defaultMethod = "Consumer";

        public Consumer(
                IMassTransitProducer massTransitProducer,
                IRawTransactionService rawTransactionService,
                ITransactionService transactionService,
                IIPDBService ipdbService,
                IAppSettings appSettings,
                ILogger logger,
                ILogEvent logEvent
        )
        {
            this.massTransitProducer = massTransitProducer;
            this.rawTransactionService = rawTransactionService;
            this.transactionService = transactionService;
            this.ipdbService = ipdbService;
            this.appSettings = appSettings;
            this.logger = logger;
            this.logEvent = logEvent;
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

            ipdbService.TransformTransactionByIps(transactionModels);

            await transactionService.InsertRawTransaction(transactionModels, appSettings.IsExtraSub);

            _ = AdditionalProcessingNotAwait(transactionModels);
        }

        private async Task AdditionalProcessingNotAwait(IEnumerable<RawTransactionModel> transactionModels)
        {
            var tasks = new List<Task>
            {
                InsertLogTrail(transactionModels),
                HandleClearRedis(transactionModels),
                PushTransactionToMonitorAsync(transactionModels)
            };

            await Task.WhenAll(tasks);
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
            if (appSettings.IsEnableLogTrail)
            {
                try
                {
                    var logEvents = transactions.Select(x => new LogEventInfo
                    {
                        EventDate = DateTime.Now.Date,
                        SubscriberID = x.SubscriberId,
                        SystemName = appSettings.SystemName,
                        Source = defaultSource,
                        Destination = defaultDestination,
                        Method = defaultMethod
                    });

                    _ = Task.WhenAll(logEvents.Select(async log =>
                    {
                        await logEvent.WriteEventInfoAsync(log);
                    }));
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
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
