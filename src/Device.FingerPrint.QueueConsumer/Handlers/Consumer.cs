using Device.FingerPrint.Features.ClientTransaction.Model;
using Device.FingerPrint.Features.ClientTransaction.Services;
using Device.FingerPrint.Features.ServerTransaction.Model;
using Device.FingerPrint.Features.ServerTransaction.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.Queue;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.QueueConsumer.Handlers
{
    public class Consumer : BackgroundService
    {
        private EasyNetQConnector _rabbitMQConsume;
        private readonly ISentryClient sentryClient;
        private readonly IRawTransactionService rawTransactionService;
        private readonly IClientTransactionService fingerPrintService;

        private readonly int consumerBatchSize;
        private readonly List<TransactionEntity> listServerTrans;
        private readonly List<ClientTransactionEntity> listClientTrans;

        public Consumer(IConfiguration _configuration, ISentryClient sentryClient, IRawTransactionService rawTransactionService, IClientTransactionService fingerPrintService)
        {
            this.sentryClient = sentryClient;
            this.rawTransactionService = rawTransactionService;
            this.fingerPrintService = fingerPrintService;
            consumerBatchSize = int.Parse(_configuration["Consumer:BatchSize"]);

            ConsumeMessageBus(_configuration);

            listServerTrans = new List<TransactionEntity>();
            listClientTrans = new List<ClientTransactionEntity>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _rabbitMQConsume.AdvanceBus.Consume<TransactionModel>(_rabbitMQConsume.Queue, (message, info) =>
                {
                    var messageBody = message.Body;
                    var json = JsonConvert.SerializeObject(messageBody);
                    listServerTrans.Add(TranformTransactionEntity(messageBody));
                    if (listServerTrans.Count >= consumerBatchSize)
                    {
                        rawTransactionService.InsertServerTransaction(listServerTrans);

                        listServerTrans.Clear();
                    }
                    // Console.WriteLine("TransactionModel {0}", json);
                });

                _rabbitMQConsume.AdvanceBus.Consume<ClientTransactionModel>(_rabbitMQConsume.Queue, (message, info) =>
                {
                    var messageBody = message.Body;
                    var json = JsonConvert.SerializeObject(messageBody);
                    listClientTrans.Add(TranformClientTransactionEntity(messageBody));
                    if (listClientTrans.Count >= consumerBatchSize)
                    {
                        fingerPrintService.InsertClientTransaction(listClientTrans);

                        listClientTrans.Clear();
                    }
                    // Console.WriteLine("ClientTransactionModel {0}", json);
                });
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
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
                CustId = model.CustId,
                SubAccId = model.SubAccId
            };
        }

        private static ClientTransactionEntity TranformClientTransactionEntity(ClientTransactionModel model)
        {
            return new ClientTransactionEntity
            {
                Attribute = model.Attribute,
                CookieCode = model.CookieCode,
                UserAgentAttribute = model.UserAgentAttribute,
                UserAgentCode = model.UserAgentCode,
                FingerPrintCode = model.FingerPrintCode,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                Flagged = model.Flagged,
                Url = model.Url,
                TransTime = model.TransTime
            };
        }

        private void ConsumeMessageBus(IConfiguration configuration)
        {

            _rabbitMQConsume = new EasyNetQConnector(configuration["RabbitMQ:Host"] + ":" + configuration["RabbitMQ:Port"],
                                      configuration["RabbitMQ:VirtualHost"],
                                      int.Parse(configuration["RabbitMQ:PrefetchCount"].ToString()),
                                      configuration["RabbitMQ:UserName"],
                                      configuration["RabbitMQ:Password"]);

            _rabbitMQConsume.CreateBus()
                    .DeclareExchange(configuration["RabbitMQ:Exchange"])
                    .DeclareQueue(configuration["RabbitMQ:Name"])
                    .DeclareRoutingKey(configuration["RabbitMQ:RoutingKey"])
                    .Bind();
        }

    }
}
