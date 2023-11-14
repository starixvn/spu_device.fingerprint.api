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
    using Device.FingerPrint.QueueConsumer.Services;
    using Fanex.Logging;
    using Fanex.Oneyed.Infrastructure.Models;
    using MassTransit;

    public class QueueConsumer : IConsumer<Batch<MessageQueue>>
    {
        private readonly IRawTransactionService rawTransactionService;
        private readonly ILogTrailClientService logTrailClientService;
        private readonly ITransactionService transactionService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        private const string defaultDestination = "Device.FingerPrint.QueueConsumer";
        private const string defaultMethod = "Consumer";

        public QueueConsumer(
                IRawTransactionService rawTransactionService,
                ILogTrailClientService logTrailClientService,
                ITransactionService transactionService,
                IAppSettings appSettings,
                ILogger logger
                )
        {
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
                if (appSettings.GetValue<bool>("RabbitMQ:Newfps:Queue-Consume"))
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
            var transactionModels = new List<RawTransactionModel>();

            foreach (var message in messages)
            {
                var transaction = rawTransactionService.TransformRawTransaction(message.Message);

                await rawTransactionService.GetTransactionAttribute(transaction);

                transactionModels.Add(transaction);
            }

            var transOfExtraSubs = transactionModels.Where(x => IsTransactionOfExtraSubs(x.SubscriberName));
            var transOfDCSSubs = transactionModels.Where(x => !IsTransactionOfExtraSubs(x.SubscriberName));

            if (transOfExtraSubs.Any())
            {
                rawTransactionService.TransformTransactionByIps(transOfExtraSubs);

                await transactionService.InsertRawTransaction(transOfExtraSubs, true);

                await InsertLogTrail(transOfExtraSubs);
            }

            if (transOfDCSSubs.Any())
            {
                await transactionService.InsertRawTransaction(transOfDCSSubs, false);

                await InsertLogTrail(transOfDCSSubs);
            }

            rawTransactionService.ClearRedisAsync(transactionModels);
        }

        private bool IsTransactionOfExtraSubs(string subscriber)
        {
            var extraSubs = appSettings.ExtraSubs;
            if (appSettings.IsReadyDbExtra && !string.IsNullOrWhiteSpace(extraSubs))
            {
                return extraSubs.Split(';').Any(x => x.Equals(subscriber, StringComparison.CurrentCultureIgnoreCase));
            }

            return false;
        } 

        private async Task InsertLogTrail(IEnumerable<RawTransactionModel> transactions)
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
    }
}
