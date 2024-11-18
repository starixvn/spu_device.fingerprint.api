namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Encoders;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.QueueConsumer.Services;
    using MassTransit;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class Consumer : IConsumer<Batch<MessageQueue>>
    {
        private readonly IRawTransactionService rawTransactionService;        
        private readonly ITransactionService transactionService;
        private readonly IIPDBService ipdbService;
        private readonly IAppSettings appSettings;
        private readonly ILogstashService logstashService;

        private const string hashKeyEverTagging = "EverTagging";

        public Consumer(
                IRawTransactionService rawTransactionService,                
                ITransactionService transactionService,
                IIPDBService ipdbService,
                IAppSettings appSettings,
                ILogstashService logstashService
        )
        {
            this.rawTransactionService = rawTransactionService;            
            this.transactionService = transactionService;
            this.ipdbService = ipdbService;
            this.appSettings = appSettings;
            this.logstashService = logstashService;
        }

        public Task Consume(ConsumeContext<Batch<MessageQueue>> context)
        {
            try
            {
                if (appSettings.GetValue<bool>("Consumer:IsConsume"))
                {
                    var rawTransModels = TransformRawTransactionModel(context.Message);

                    if (rawTransModels != null && rawTransModels.Any())
                    {
                        if (appSettings.IsExtraSub)
                        {
                            rawTransModels = ipdbService.TransformTransactionByIps(rawTransModels);
                        }

                        rawTransactionService.InsertRawTransactionDetail(rawTransModels, appSettings.IsExtraSub);

                        transactionService.InsertRawTransaction(rawTransModels, appSettings.IsExtraSub);

                        _ = AdditionalProcessingNotAwait(rawTransModels);
                    }
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _ = QueueConsumer.Utilities.LogErrorAsync(ex);
                throw;
            }
        }

        private IEnumerable<RawTransactionModel> TransformRawTransactionModel(Batch<MessageQueue> messages)
        {
            var transformedDataBag = new ConcurrentBag<RawTransactionModel>();

            Parallel.ForEach(messages, new ParallelOptions { MaxDegreeOfParallelism = 10 }, message =>
            {
                if (message?.Message != null
                    && !string.IsNullOrEmpty(message.Message.Account)
                    && !Utilities.ContainsUnicodeCharacter(message.Message.Account))
                {
                    try
                    {
                        var rawTransModel = rawTransactionService
                                                .TransformRawTransaction(message.Message)
                                                .GetRawTransAttributes();                       

                        transformedDataBag.Add(rawTransModel);
                    }
                    catch (Exception ex)
                    {
                        _ = QueueConsumer.Utilities.LogErrorAsync(ex);
                    }
                }
            });

            return transformedDataBag;
        }

        private async Task AdditionalProcessingNotAwait(IEnumerable<RawTransactionModel> transactionModels)
        {
            try
            {
                var everTagging = transactionModels
                                .Where(x => !string.IsNullOrEmpty(x.FPTagging))
                                .Select(x => DeviceEncoder.Encrypt(x.FPTagging));

                if (everTagging.Any())
                {
                    rawTransactionService.InsertOrUpdateEverTagging(hashKeyEverTagging, everTagging, appSettings.EverTaggingExpireSecond);
                }

                await InsertLogstash(transactionModels);
            }
            catch (Exception ex)
            {
                _ = QueueConsumer.Utilities.LogErrorAsync(ex);
            }
        }

        private async Task InsertLogstash(IEnumerable<RawTransactionModel> transactions)
        {
            if (appSettings.IsEnableLogstash)
            {
                try
                {
                    await Task.WhenAll(transactions.Select(logstashService.PushLog));
                }
                catch (Exception ex)
                {
                    _ = QueueConsumer.Utilities.LogErrorAsync(ex);
                }
            }
        }
    }
}
