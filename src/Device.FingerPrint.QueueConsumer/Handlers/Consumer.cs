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
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Schema;
    using Sentry;
    using Sentry.Protocol;
    using System;
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
        private readonly ISentryClient sentryClient;

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
                        , IBrowserInfoService browserInfoService
                        , ISentryClient sentryClient)
        {
            this.rawTransactionService = rawTransactionService;
            this.fingerPrintService = fingerPrintService;
            this.appSettings = appSettings;
            this.transactionReportServices = transactionReportServices;
            this.browserInfoService = browserInfoService;
            this.sentryClient = sentryClient;

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

        private static FingerprintAttributesEntity TranformFingerprintAttributesEntity(FingerprintAttributesModel model)
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
                UserAgent = model.UserAgent
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
                            var serverTransaction = TranformTransactionEntity(message.Body);

                            listServerTrans.Add(serverTransaction);

                            if (listServerTrans.Count >= appSettings.ServerTransBatchSize)
                            {
                                rawTransactionService.InsertServerTransaction(listServerTrans);

                                var groupTrans = listServerTrans.GroupBy(x => new { mdyDate = x.TransTime.ToString("MM/dd/yyyy"), x.SubscriberId })
                                                .Select(x => new TransactionSumEntity { mdyDate = x.Key.mdyDate, SubscriberId = x.Key.SubscriberId, CountTrans = x.Count() })
                                                .ToList();

                                Task.Run(() => InsertServerTransactionDailySum(groupTrans));

                                listServerTrans.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + JsonConvert.SerializeObject(message.Body), SentryLevel.Error);
                        }
                    })
                    .Add<ClientTransactionModel>((message, info) =>
                    {
                        try
                        {
                            var clientTrans = TranformClientTransactionEntity(message.Body);

                            if (!listClientTrans.Exists(x => x.SessionKey == clientTrans.SessionKey))
                            {
                                listClientTrans.Add(clientTrans);
                            }

                            if (listClientTrans.Count >= appSettings.ClientTransBatchSize)
                            {
                                fingerPrintService.InsertClientTransaction(listClientTrans);

                                var groupTrans = listClientTrans.GroupBy(x => new { mdyDate = x.TransTime.ToString("MM/dd/yyyy"), x.SubscriberId })
                                                .Select(x => new TransactionSumEntity { mdyDate = x.Key.mdyDate, SubscriberId = x.Key.SubscriberId, CountTrans = x.Count() })
                                                .ToList();

                                Task.Run(() => InsertClientTransactionDailySum(groupTrans));

                                listClientTrans.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex.Message + "\n" + JsonConvert.SerializeObject(message.Body), SentryLevel.Error);
                        }
                    })
                    .Add<FingerprintAttributesModel>((message, info) =>
                    {                        
                        try
                        {                            
                            listFpAttributes.Add(TranformFingerprintAttributesEntity(message.Body));

                            if (listFpAttributes.Count >= appSettings.FPAttributeBatchSize)
                            {
                                fingerPrintService.InsertFingerprintAttributes(listFpAttributes);

                                listFpAttributes.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureMessage(ex + "\n" + JsonConvert.SerializeObject(listFpAttributes), SentryLevel.Error);
                        }
                    })
                    .Add<BrowserInfoModel>((message, info) =>
                    {
                        try
                        {
                            listBrowserInfo.Add(TranformBrowserInfoEntityEntity(message.Body));

                            if (listBrowserInfo.Count >= appSettings.BrowserInfoBatchSize)
                            {                               
                                try
                                {
                                    browserInfoService.InsertBrowserInfo(listBrowserInfo);
                                }
                                catch (Exception ex)
                                {
                                    sentryClient.CaptureException(ex);                                    
                                }

                                listBrowserInfo.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            sentryClient.CaptureException(ex);                            
                        }
                    })
                    .Add<ClientTransactionLogsModel>((message, info) =>
                    {
                        try
                        {
                            var clientTrans = TranformClientTransactionLogsEntity(message.Body);

                            if (!listClientTransLogs.Exists(x => x.SessionKey == clientTrans.SessionKey))
                            {
                                listClientTransLogs.Add(clientTrans);
                            }

                            if (listClientTransLogs.Count >= appSettings.ClientTransLogsBatchSize)
                            {
                                fingerPrintService.InsertClientTransactionLogs(listClientTransLogs);

                                listClientTransLogs.Clear();
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
            try
            {
                if (groupTrans != null && groupTrans.Any())
                {
                    var json = JsonConvert.SerializeObject(groupTrans);
                    transactionReportServices.InsertServerTransactionDailySum(json);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        private void InsertClientTransactionDailySum(IEnumerable<TransactionSumEntity> groupTrans)
        {
            try
            {
                if (groupTrans != null && groupTrans.Any())
                {
                    var json = JsonConvert.SerializeObject(groupTrans);
                    transactionReportServices.InsertClientTransactionDailySum(json);
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming();

            return Task.CompletedTask;
        }
    }
}
