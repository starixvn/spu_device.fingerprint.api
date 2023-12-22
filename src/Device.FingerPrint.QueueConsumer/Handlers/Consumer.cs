namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Model.LogEvent;
    using Device.FingerPrint.Core.Services;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Model.TransactionMonitoring;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.QueueConsumer.Services;
    using Device.FingerPrint.QueueConsumer.Utility;
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
        private readonly ICrypto crypto;

        public Consumer(
                IMassTransitProducer massTransitProducer,
                IRawTransactionService rawTransactionService,
                ITransactionService transactionService,
                IIPDBService ipdbService,
                IAppSettings appSettings,
                ILogger logger,
                ILogEvent logEvent,
                ICrypto crypto
        )
        {
            this.massTransitProducer = massTransitProducer;
            this.rawTransactionService = rawTransactionService;
            this.transactionService = transactionService;
            this.ipdbService = ipdbService;
            this.appSettings = appSettings;
            this.logger = logger;
            this.logEvent = logEvent;
            this.crypto = crypto;
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
            catch (Exception)
            {
                throw;
            }
        }

        private async Task ProcessTransaction(Batch<MessageQueue> messages)
        {
            var transactionModels = await Task.WhenAll(messages.Select(async x =>
            {
                return await ProcessTransformRawTran(x.Message);
            }));

            await ProcessInsertRawTransaction(transactionModels);

            _ = AdditionalProcessingNotAwait(transactionModels);
        }

        private async Task<RawTransactionModel> ProcessTransformRawTran(MessageQueue message)
        {
            var eventCode = GetEventCode(message.SubscriberId, LogEventCode.MethodTransformRawTran);
            var eventCodeDetail = GetEventCode(message.SubscriberId, LogEventCode.MethodTransformRawTran, message.Account);

            try
            {
                var rawTransactions = await rawTransactionService.TransformRawTransaction(message);

                _ = InsertLogTrail(rawTransactions, eventCode, eventCodeDetail, LogEventCode.MethodTransformRawTran);

                return rawTransactions;
            }
            catch (Exception ex)
            {
                LogError($"{ex.Message} \r\n Event Code = {eventCode} \r\n Event Code Detail = {eventCodeDetail}", ex);
                throw;
            }
        }

        private async Task ProcessInsertRawTransaction(RawTransactionModel[] transactionModels)
        {
            List<string> eventCodes = null;
            List<string> eventCodesDetail = null;

            try
            {
                eventCodes = transactionModels.Select(x => GetEventCode(x.SubscriberId, LogEventCode.MethodInsertTran)).ToList();
                eventCodesDetail = transactionModels.Select(x => GetEventCode(x.SubscriberId, LogEventCode.MethodInsertTran, x.LoginName)).ToList();

                ipdbService.TransformTransactionByIps(transactionModels);

                await transactionService.InsertRawTransaction(transactionModels, appSettings.IsExtraSub);

                _= InsertLogTrails(transactionModels.ToList(), eventCodes, eventCodesDetail);
            }
            catch (Exception ex)
            {
                LogError($"{ex.Message} \r\n Event Code = {string.Join(", ", eventCodes)} \r\n Event Code Detail = {string.Join(", ", eventCodesDetail)}", ex);
                throw;
            }
        }

        private async Task InsertLogTrails(List<RawTransactionModel> transactionModels, List<string> eventCodes, List<string> eventCodesDetails)
        {
            for (int i = 0; i <= transactionModels.Count(); i++)
            {
                var transaction = transactionModels[i];
                var eventCode = eventCodes[i];
                var eventCodesDetail = eventCodesDetails[i];

                await InsertLogTrail(transaction, eventCode, eventCodesDetail, LogEventCode.MethodInsertTran);
            }
        }

        private async Task AdditionalProcessingNotAwait(IEnumerable<RawTransactionModel> transactionModels)
        {
            var tasks = new List<Task>
            {
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
                LogError(ex.Message, ex);
            }
        }

        private async Task InsertLogTrail(RawTransactionModel transactionModel, string eventCode, string eventCodeDetail,string method)
        {
            if (appSettings.IsEnableLogTrail)
            {
                try
                {
                    var logEventInfor = new LogEventInfo
                    {
                        EventDate = DateTime.Now.Date,
                        SubscriberID = transactionModel.SubscriberId,
                        LoginName = transactionModel.LoginName,
                        TransTime = DateTime.Now,
                        EventCode = eventCode,
                        EventCodeDetail = eventCodeDetail,
                        SystemName = appSettings.SystemName,
                        Source = LogEventCode.Source,
                        Destination = LogEventCode.Destination,
                        Method = method
                    };

                    await logEvent.WriteEventInfoAsync(logEventInfor);
                }
                catch (Exception ex)
                {
                    LogError(ex.Message, ex);
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

        private void LogError(string message, Exception ex)
        {
            var logMessage = new LogMessage(message)
                   .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                   .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                   .ToString();

            logger.Error(logMessage);
        }

        private string GetEventCode(int subscriberId, string method, string loginName = null)
        {
            var eventCode = string.Concat(DateTime.Now.ToString("yyyy/MM/dd"), subscriberId, appSettings.SystemName, LogEventCode.Source, LogEventCode.Destination, method, loginName);
            return crypto.GetMD5Hash(eventCode);
        }
    }
}
