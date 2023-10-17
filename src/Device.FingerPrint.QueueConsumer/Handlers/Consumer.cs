namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Features.Transaction.Model;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Queue;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using Fanex.Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Consumer : BackgroundService
    {
        private readonly ITransactionService transactionService;
        private readonly IAppSettings appSettings;
        private readonly ILogger _logger;

        private readonly List<TransactionEntity> listTransactionInsertModel;

        public Consumer(IAppSettings appSettings
                        , ITransactionService transactionService
                        , ILogger logger)
        {
            this.transactionService = transactionService;
            this.appSettings = appSettings;
            this._logger = logger;

            listTransactionInsertModel = new List<TransactionEntity>();
        }

        private static TransactionEntity TranformTransactionEntity(TransactionModel model)
        {
            return new TransactionEntity
            {
                LoginName = model.LoginName,
                ClientIP = model.ClientIP,
                FakeIP = model.FakeIP,
                UserAgent = model.UserAgent,
                UserAgentCode = model.UserAgentCode,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                TransKey = model.TransKey,
                TransTime = model.TransTime,
                Url = model.Url,
                Action = model.Action,
                CustId = model.CustId,
                SubAccId = model.SubAccId,
                CookieCode = model.CookieCode,
                FingerprintAttribute = model.FingerprintAttribute,
                UserAgentInfo = model.UserAgentInfo,
                BrowserInfo = model.BrowserInfo,
                BrowserInfoAttribute = model.BrowserInfoAttribute
            };
        }

        private void StartConsuming()
        {
            var easyNetQConnector = RabbitMQBus();
            _logger.Info("Start Consuming!");

            try
            {
                easyNetQConnector.AdvanceBus.Consume(easyNetQConnector.Queue, x =>
                {
                    x.Add<TransactionModel>((message, info) =>
                    {
                        var transaction = TranformTransactionEntity(message.Body);
                        listTransactionInsertModel.Add(transaction);

                        if (listTransactionInsertModel.Count >= appSettings.TransBatchSize)
                        {
                            try
                            {
                                transactionService.InsertTransaction(listTransactionInsertModel);

                                listTransactionInsertModel.Clear();
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex.Message + "\n" + JsonConvert.SerializeObject(listTransactionInsertModel));
                            }
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming();

            return Task.CompletedTask;
        }
    }
}
