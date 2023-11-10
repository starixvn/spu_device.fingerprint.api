namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.QueueConsumer.Factory;
    using Device.FingerPrint.QueueConsumer.Services;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using Fanex.Logging;
    using Fanex.Oneyed.Infrastructure.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Consumer : BackgroundService
    {
        private readonly IRawTransactionService rawTransactionService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;

        private readonly DCSSubsTransactionFactory _dCSSubsTransactionFactory;
        private readonly ExtraSubsTransactionFactory _extraSubsTransactionFactory;

        private readonly List<RawTransactionModel> transDcsSub = new List<RawTransactionModel>();
        private readonly List<RawTransactionModel> tranOfExtraSubs = new List<RawTransactionModel>();

        public Consumer(IAppSettings appSettings
                        , ITransactionService transactionService
                        , IRawTransactionService rawTransactionService
                        , ILogger logger)
        {
            this.rawTransactionService = rawTransactionService;
            this.appSettings = appSettings;
            this.logger = logger;

            _dCSSubsTransactionFactory = new DCSSubsTransactionFactory(appSettings, transactionService);
            _extraSubsTransactionFactory = new ExtraSubsTransactionFactory(appSettings);

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming();

            return Task.CompletedTask;
        }

        private void StartConsuming()
        {
            var easyNetQConnector = RabbitMQBus();
            logger.Info("Start Consuming!");

            try
            {
                easyNetQConnector.AdvanceBus.Consume(easyNetQConnector.Queue, x =>
                {
                    x.Add<MessageQueue>((message, info) =>
                    {
                        try
                        {
                            ProcessTransaction(message.Body);
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.Message + "\n" + JsonConvert.SerializeObject(message.Body));
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private async void ProcessTransaction(MessageQueue message)
        {
            var transaction = rawTransactionService.TransformRawTransaction(message);
            await rawTransactionService.GetTransactionAttribute(transaction);
            var isTransOfExtraSubs = IsTransactionOfExtraSubs(transaction.SubscriberName);
            if (isTransOfExtraSubs)
            {
                HandleAddTransOfExtraSubs(transaction);
            }
            else
            {
                HandleAddTransOfDCSSubs(transaction);
            }
        }

        private bool IsTransactionOfExtraSubs(string subscriber)
        {
            //var extraSubs = AppSettings.ExtraSubs;
            //if (AppSettings.IsReadyDbExtra && !string.IsNullOrWhiteSpace(extraSubs))
            //{
            //    return extraSubs.Split(';').Any(x => x.Equals(subscriber, StringComparison.CurrentCultureIgnoreCase));
            //}

            return false;
        }

        private void HandleAddTransOfExtraSubs(RawTransactionModel tran)
        {
            tranOfExtraSubs.Add(tran);
            _extraSubsTransactionFactory.HandleTransaction(tranOfExtraSubs);
            rawTransactionService.ClearRedis(tranOfExtraSubs);
        }

        private void HandleAddTransOfDCSSubs(RawTransactionModel tran)
        {
            transDcsSub.Add(tran);
            _dCSSubsTransactionFactory.HandleTransaction(transDcsSub);
            if (transDcsSub.Count >= appSettings.TransBatchSize)
            {
                //PushTransactionToMonitor(transDcsSub);
                rawTransactionService.ClearRedis(transDcsSub);
                transDcsSub.Clear();
            }
        }

        private EasyNetQConnector RabbitMQBus()
        {
            var easyNetQConnector = new EasyNetQConnector(appSettings.Configuration["RabbitMQ:Host"] + ":" + appSettings.Configuration["RabbitMQ:Port"],
                                      appSettings.Configuration["RabbitMQ:VirtualHost"],
                                      int.Parse(appSettings.Configuration["RabbitMQ:PrefetchCount"]),
                                      appSettings.Configuration["RabbitMQ:UserName"],
                                      appSettings.Configuration["RabbitMQ:Password"]);

            easyNetQConnector
                    .CreateBus()
                    .DeclareExchange(appSettings.Configuration["RabbitMQ:Exchange"])
                    .DeclareQueue(appSettings.Configuration["RabbitMQ:Name"])
                    .DeclareRoutingKey(appSettings.Configuration["RabbitMQ:RoutingKey"])
                    .Bind();

            return easyNetQConnector;
        }
    }
}
