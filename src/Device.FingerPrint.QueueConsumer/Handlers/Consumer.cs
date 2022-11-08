namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Features.BrowserInfo.Model;
    using Device.FingerPrint.Features.BrowserInfo.Services;
    using Device.FingerPrint.Features.ClientTransaction.Model;
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
        private readonly IClientTransactionService fingerPrintService;
        private readonly ITransactionReportServices transactionReportServices;
        private readonly IBrowserInfoService browserInfoService;
        

        private readonly List<TransactionModel> listTransactionInsertModel;
        private readonly List<TransactionEntity> listServerTrans;
        private readonly List<ClientTransactionEntity> listClientTrans;
        private readonly List<ClientTransactionLogsEntity> listClientTransLogs;
        private readonly List<FingerprintAttributesEntity> listFpAttributes;
        private readonly List<BrowserInfoEntity> listBrowserInfo;
        private readonly IAppSettings appSettings;

        public Consumer(IAppSettings appSettings
                        , IRawTransactionService rawTransactionService
                        , IClientTransactionService fingerPrintService
                        , ITransactionReportServices transactionReportServices
                        , IBrowserInfoService browserInfoService)
        {
            this.rawTransactionService = rawTransactionService;
            this.fingerPrintService = fingerPrintService;
            this.appSettings = appSettings;
            this.transactionReportServices = transactionReportServices;
            this.browserInfoService = browserInfoService;           

            listTransactionInsertModel = new List<TransactionModel>();
            listServerTrans = new List<TransactionEntity>();
            listClientTrans = new List<ClientTransactionEntity>();
            listFpAttributes = new List<FingerprintAttributesEntity>();
            listBrowserInfo = new List<BrowserInfoEntity>();
            listClientTransLogs = new List<ClientTransactionLogsEntity>();
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
                SubAccId = model.SubAccId
            };
        }

        private static ClientTransactionEntity TranformClientTransactionEntity(ClientTransactionModel model)
        {
            return new ClientTransactionEntity
            {
                CookieCode = model.CookieCode,
                FingerPrintCode = model.FingerPrintCode,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                Url = model.Url,
                AddonCode = model.AddonCode,
                TransTime = model.TransTime
            };
        }

        private static ClientTransactionLogsEntity TranformClientTransactionLogsEntity(ClientTransactionLogsModel model)
        {
            return new ClientTransactionLogsEntity
            {
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                Url = model.Url,
                UserAgent = model.UserAgent,
                TransTime = model.TransTime,
                Information = model.Information
            };
        }

        private static FingerprintAttributesEntity TranformFingerprintAttributesEntity(FingerprintAttributeModel model)
        {
            return new FingerprintAttributesEntity
            {
                FingerPrintCode = model.FingerPrintCode,
                Attribute = model.Attribute,
                Accept = model.Accept,
                Encoding = model.Encoding,
                Headers = model.Headers,
                OSName = model.OSName,
                BrowserName = model.BrowserName,
                BrowserVersion = model.BrowserVersion,
                UserAgent = model.UserAgent,
                Platform = model.Platform,
                BrowserInstanceCode = model.BrowserInstanceCode
            };
        }

        private static BrowserInfoEntity TranformBrowserInfoEntityEntity(BrowserInfoModel model)
        {
            return new BrowserInfoEntity
            {
                SessionKey = model.SessionKey,
                HeadlessScore = model.HeadlessScore,
                BrowserInfo = model.BrowserInfo,
                BrowserInfoAttribute = model.BrowserInfoAttribute,
                BrowserInfoAttributeCode = model.BrowserInfoAttributeCode
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
                            listTransactionInsertModel.Add(message.Body);

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
