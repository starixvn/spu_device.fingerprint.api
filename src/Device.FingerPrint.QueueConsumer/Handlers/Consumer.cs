namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Model;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.QueueConsumer.Services;
    using Fanex.Logging;
    using Fanex.Logging.Extensions;
    using Fanex.Logging.Extensions.Database;
    using Fanex.LogTrail.Service;
    using MassTransit;
    using Microsoft.IdentityModel.Tokens;
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

            var filterTransaction = transactionModels.Where(x => x.TransStatus != (int)TransStatusEnum.InvalidLoginName);

            ipdbService.TransformTransactionByIps(filterTransaction);

            await transactionService.InsertRawTransaction(filterTransaction, appSettings.IsExtraSub);

            _ = AdditionalProcessingNotAwait(filterTransaction);
        }

        private async Task AdditionalProcessingNotAwait(IEnumerable<RawTransactionModel> transactionModels)
        {
            var tasks = new List<Task>
            {
                InsertLogTrail(transactionModels),
                //HandleClearRedis(transactionModels),
                //PushTransactionToMonitorAsync(transactionModels)
            };

            var rawJsChallenges = transactionModels.Where(x => !string.IsNullOrEmpty(x.RawJsChallengeInfo?.ChallengeCode))?.Select(x => x.RawJsChallengeInfo?.ChallengeCode);
            if (rawJsChallenges != null && rawJsChallenges.Any())
            {
                tasks.Add(HandleClearRedisJsChallenge(rawJsChallenges));
            }

            await Task.WhenAll(tasks);
        }

        private async Task HandleClearRedisJsChallenge(IEnumerable<string> rawJsChallenges)
        {
            try
            {
                await rawTransactionService.ClearRedisJsChallenge(rawJsChallenges);

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

                    await Task.WhenAll(logEvents.Select(async log =>
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
