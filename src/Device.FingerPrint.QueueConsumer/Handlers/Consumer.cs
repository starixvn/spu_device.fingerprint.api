namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Features.BrowserInfo.Services;
    using Device.FingerPrint.Features.ClientTransaction.Services;
    using Device.FingerPrint.Features.Monitor.Model;
    using Device.FingerPrint.Features.Monitor.Services;
    using Device.FingerPrint.Features.ServerTransaction.Model;
    using Device.FingerPrint.Features.ServerTransaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Queue;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using Fanex.Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class Consumer : BackgroundService
    {
        private readonly IRawTransactionService rawTransactionService;
        private readonly ITransactionReportServices transactionReportServices;
        

        private readonly List<TransactionEntity> listTransactionInsertModel;
        private readonly IAppSettings appSettings;

        public Consumer(IAppSettings appSettings
                        , IRawTransactionService rawTransactionService
                        , IClientTransactionService fingerPrintService
                        , ITransactionReportServices transactionReportServices
                        , IBrowserInfoService browserInfoService)
        {
            this.rawTransactionService = rawTransactionService;
            this.appSettings = appSettings;
            this.transactionReportServices = transactionReportServices;

            listTransactionInsertModel = new List<TransactionEntity>();
        }
        

        private static TransactionEntity TranformTransactionEntity(TransactionModel model)
        {
            return new TransactionEntity
            {
                LoginName = model.LoginName,
                ClientIp = model.ClientIp,
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
                Platform = model.Platform,
                BrowserInstanceCode = model.BrowserInstanceCode
                BrowserInfo = model.BrowserInfo,
                BrowserInfoAttribute = model.BrowserInfoAttribute
            };
        }

        private void StartConsuming()
        {
            try
            {
                var easyNetQConnector = RabbitMQBus();

                easyNetQConnector.AdvanceBus.Consume(easyNetQConnector.Queue, x =>
                {
                    x.Add<TransactionModel>((message, info) =>
                    {
                        try
                        {
                            var transaction = TranformTransactionEntity(message.Body);
                            listTransactionInsertModel.Add(transaction);

                            if (listTransactionInsertModel.Count >= appSettings.TransBatchSize)
                            {
                                rawTransactionService.InsertTransaction(listTransactionInsertModel);

                                listTransactionInsertModel.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + JsonConvert.SerializeObject(message.Body), SentryLevel.Error);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
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

        private void InsertServerTransactionDailySum(IEnumerable<TransactionSumEntity> groupTrans)
        {
            if (groupTrans != null && groupTrans.Any())
            {
                var json = JsonConvert.SerializeObject(groupTrans);
                transactionReportServices.InsertServerTransactionDailySum(json);
            }
        }

        private void InsertClientTransactionDailySum(IEnumerable<TransactionSumEntity> groupTrans)
        {
            if (groupTrans != null && groupTrans.Any())
            {
                var json = JsonConvert.SerializeObject(groupTrans);
                transactionReportServices.InsertClientTransactionDailySum(json);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming();

            return Task.CompletedTask;
        }
    }
}
