namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Features.Transaction.Model;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.QueueConsumer.Services;
    using Device.FingerPrint.Redis;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using Fanex.Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class Consumer : BackgroundService
    {
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;
        private readonly IRedisService redisService;
        private readonly IBrowserInfoService browserInfoService;

        private readonly List<TransactionEntity> listTransactionInsertModel;

        public Consumer(IAppSettings appSettings
                        , ITransactionService transactionService
                        , ILogger logger
                        , IRedisService redisService
                        , IBrowserInfoService browserInfoService
                        , IRawTransactionService rawTransactionService)
        {
            this.transactionService = transactionService;
            this.appSettings = appSettings;
            this.logger = logger;
            this.redisService = redisService;
            this.browserInfoService = browserInfoService;

            listTransactionInsertModel = new List<TransactionEntity>();
            this.rawTransactionService = rawTransactionService;

        }

        private async Task GetTransactionAttribute(TransactionModel model)
        {
            try
            {
                if (model != null)
                {

                    var hashKey = $"{model.SubscriberId}:{model.SessionKey}";

                    var fingerPrintTask = redisService.HashGetAsync<FingerprintModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint));
                    var headerTask = redisService.HashGetAsync<HeaderModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.Header));
                    var browserInfoTask = redisService.HashGetAsync<BrowserDetectModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.BrowserInfo));
                    var cookieTask = redisService.HashGetAsync<string>(hashKey, RedisString.GetRedisKeyName(RedisKey.Cookie));
                    var headlessTask = redisService.HashGetAsync<HeadlessBrowserModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.Headless));

                    await Task.WhenAll(fingerPrintTask, headerTask, browserInfoTask, cookieTask, headlessTask);

                    var fingerPrint = fingerPrintTask.Result;
                    var header = headerTask.Result;
                    var browserInfo = browserInfoTask.Result;
                    var cookie = cookieTask.Result;
                    var headless = headlessTask.Result;

                    var browserInfoModel = browserInfoService.GetBrowserInfo(header, browserInfo);

                    if (headless != null)
                    {
                        browserInfoModel.HeadlessScore = headless.Score;
                        browserInfoModel.HeadlessDetail = headless.TestsDetail;
                    }

                    var userAgentInfo = rawTransactionService.ParseUserAgentInfo(model.UserAgent);
                    var browserInfoAttributeModel = browserInfoService.GetBrowserInfoAttribute(header, browserInfo);

                    model.BrowserInfoAttribute = browserInfoAttributeModel;
                    model.FingerprintAttribute = rawTransactionService.TransformFingerprintAttributes(fingerPrint, header, browserInfo, userAgentInfo);
                    model.CookieCode = cookie;
                    model.UserAgentInfo = userAgentInfo;
                    model.BrowserInfo = browserInfoModel;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
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
            logger.Info("Start Consuming!");

            try
            {

                easyNetQConnector.AdvanceBus.Consume(easyNetQConnector.Queue, x =>
                {
                    x.Add<TransactionModel>(async (message, info) =>
                    {
                        await GetTransactionAttribute(message.Body);

                        var transaction = TranformTransactionEntity(message.Body);

                        listTransactionInsertModel.Add(transaction);

                        if (listTransactionInsertModel.Count >= appSettings.TransBatchSize)
                        {
                            try
                            {
                                transactionService.InsertTransaction(listTransactionInsertModel);

                                ClearRedis(listTransactionInsertModel);

                                listTransactionInsertModel.Clear();


                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex.Message + "\n" + JsonConvert.SerializeObject(listTransactionInsertModel));
                            }
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
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

        private void ClearRedis(List<TransactionEntity> transactions)
        {
            var groupByTrans = transactions.GroupBy(x => new
            {
                x.SubscriberId,
                x.SessionKey
            });

            foreach (var trans in groupByTrans)
            {
                var hashKey = $"{trans.Key.SubscriberId}:{trans.Key.SessionKey}";

                redisService.HashDeleteAsync(hashKey, RedisString.GetRedisKeyNames());
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming();

            return Task.CompletedTask;
        }
    }
}
