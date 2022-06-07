namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Features.ClientTransaction.Model;
    using Device.FingerPrint.Features.ClientTransaction.Services;
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
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class Consumer : BackgroundService
    {
        private readonly IRawTransactionService rawTransactionService;
        private readonly IClientTransactionService fingerPrintService;


        private readonly List<TransactionEntity> listServerTrans;
        private readonly List<ClientTransactionEntity> listClientTrans;
        private readonly List<FingerprintAttributesEntity> listFpAttributes;
        private readonly IAppSettings appSettings;

        public Consumer(IAppSettings appSettings, IRawTransactionService rawTransactionService, IClientTransactionService fingerPrintService)
        {
            this.rawTransactionService = rawTransactionService;
            this.fingerPrintService = fingerPrintService;
            this.appSettings = appSettings;


            listServerTrans = new List<TransactionEntity>();
            listClientTrans = new List<ClientTransactionEntity>();
            listFpAttributes = new List<FingerprintAttributesEntity>();

        }

        private static TransactionEntity TranformTransactionEntity(TransactionModel model)
        {
            return new TransactionEntity
            {
                LoginName = model.LoginName,
                ClientIp = model.ClientIp,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                TransKey = model.TransKey,
                TransTime = model.TransTime,
                Url = model.Url,
                Action = model.Action,
                UserAgent = model.UserAgent,
                UserAgentCode = model.UserAgentCode,
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
                RobotDetail = model.Flagged,
                Flagged = TransformRobotDetectionDetail(model.Flagged).Score > 0.5 ? FlaggedStatus.bot.ToString() : FlaggedStatus.human.ToString(),
                Url = model.Url,
                TransTime = model.TransTime
            };
        }

        private static FingerprintAttributesEntity TranformFingerprintAttributesEntity(FingerprintAttributesModel model)
        {
            return new FingerprintAttributesEntity
            {
                FingerPrintCode = model.FingerPrintCode,
                Attribute = model.Attribute
            };
        }

        private static RobotDetectionDetail TransformRobotDetectionDetail(string robotDetectDetail)
        {
            return JsonConvert.DeserializeObject<RobotDetectionDetail>(robotDetectDetail);
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
                            var messageBody = message.Body;

                            listServerTrans.Add(TranformTransactionEntity(messageBody));

                            if (listServerTrans.Count >= appSettings.ConsumeBatchSize)
                            {
                                rawTransactionService.InsertServerTransaction(listServerTrans);

                                listServerTrans.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogService.Error(ex.Message, ex);
                        }
                    })
                    .Add<ClientTransactionModel>((message, info) =>
                    {
                        try
                        {
                            var messageBody = message.Body;

                            listClientTrans.Add(TranformClientTransactionEntity(messageBody));

                            if (listClientTrans.Count >= appSettings.ConsumeBatchSize)
                            {
                                fingerPrintService.InsertClientTransaction(listClientTrans);

                                listClientTrans.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogService.Error(ex.Message, ex);
                        }
                    })
                    .Add<FingerprintAttributesModel>((message, info) =>
                    {
                        try
                        {
                            var messageBody = message.Body;

                            listFpAttributes.Add(TranformFingerprintAttributesEntity(messageBody));

                            if (listFpAttributes.Count >= appSettings.ConsumeBatchSize)
                            {
                                fingerPrintService.InsertFingerprintAttributes(listFpAttributes);

                                listFpAttributes.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            LogService.Error(ex.Message, ex);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
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
