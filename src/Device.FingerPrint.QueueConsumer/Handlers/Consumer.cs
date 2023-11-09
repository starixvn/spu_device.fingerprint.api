namespace Device.FingerPrint.QueueConsumer.Handlers
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Features.Transaction.Model;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Queue;
    using Device.FingerPrint.QueueConsumer.Factory;
    using Device.FingerPrint.QueueConsumer.Services;
    using Device.FingerPrint.Redis;
    using DeviceDetectorNET;
    using DeviceDetectorNET.Class.Device;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using Fanex.DeviceDetector.Factory;
    using Fanex.DeviceDetector.Models;
    using Fanex.Logging;
    using Fanex.Oneyed.Infrastructure.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public class Consumer : BackgroundService
    {
        private readonly ITransactionService transactionService;
        private readonly IRawTransactionService rawTransactionService;
        private readonly IAppSettings appSettings;
        private readonly ILogger logger;
        private readonly IRedisService redisService;

        private readonly DCSSubsTransactionFactory _dCSSubsTransactionFactory;
        private readonly ExtraSubsTransactionFactory _extraSubsTransactionFactory;

        private readonly List<TransactionEntity> listTransactionInsertModel;
        private readonly List<RawTransactionModel> transDcsSub = new List<RawTransactionModel>();
        private readonly List<RawTransactionModel> tranOfExtraSubs = new List<RawTransactionModel>();

        public Consumer(IAppSettings appSettings
                        , ITransactionService transactionService
                        , ILogger logger
                        , IRedisService redisService
                        , IRawTransactionService rawTransactionService)
        {
            this.transactionService = transactionService;
            this.appSettings = appSettings;
            this.logger = logger;
            this.redisService = redisService;

            listTransactionInsertModel = new List<TransactionEntity>();
            this.rawTransactionService = rawTransactionService;

            _dCSSubsTransactionFactory = new DCSSubsTransactionFactory(appSettings, transactionService);
            _extraSubsTransactionFactory = new ExtraSubsTransactionFactory(appSettings);

        }

        private TransactionEntity TranformTransactionEntity(TransactionModel model)
        {
            var botDModel = TransformBotdModel(model.BotD);
            var flaggedCode = GetFlaggedCode(botDModel, model.UserAgent, model.FingerprintAttribute is null);

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
                FlaggedCode = flaggedCode,
                BotDetection = GetBotDetectionTrue(botDModel),
                BotComponent = GetBotComponent(botDModel, flaggedCode)
            };
        }


        private DeviceInfoModel GetDeviceInfo(string deviceCode)
        {
            if (string.IsNullOrWhiteSpace(deviceCode))
            {
                return TransformEmptyDeviceInfo();
            }

            var deviceInfo = DeviceFactory.GetDeviceInfo(deviceCode);

            if (deviceInfo != null)
            {
                return TransformDeviceInfo(deviceInfo);
            }

            return TransformEmptyDeviceInfo();
        }

        private async Task GetTransactionAttribute(RawTransactionModel model)
        {
            try
            {
                if (model != null)
                {

                    var hashKey = $"{model.SubscriberId}:{model.SessionKey}";

                    var fingerPrintTask = redisService.HashGetAsync<FingerprintModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint));
                    var headerTask = redisService.HashGetAsync<HeaderModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.Header));
                    var cookieTask = redisService.HashGetAsync<string>(hashKey, RedisString.GetRedisKeyName(RedisKey.Cookie));
                    var headlessTask = redisService.HashGetAsync<HeadlessBrowserModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.Headless));

                    await Task.WhenAll(fingerPrintTask, headerTask, cookieTask, headlessTask);

                    var fingerPrint = fingerPrintTask.Result;
                    var header = headerTask.Result;
                    var cookie = cookieTask.Result;
                    var headless = headlessTask.Result;

                    model.RawUserAgent = model.UserAgent;
                    model.RawUserAgentInfo = rawTransactionService.ParseUserAgentInfo(model.UserAgent);

                    model.FingerprintAttribute = rawTransactionService.TransformFingerprintAttributes(fingerPrint, header, model.RawUserAgentInfo);
                    model.CookieCode = cookie;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }
        }

        private DeviceInfoModel TransformEmptyDeviceInfo()
        {
            return new DeviceInfoModel
            {
                AppVersion = string.Empty,
                CapturedDate = 0,
                DeviceFPCode = string.Empty,
                DeviceNo = string.Empty,
                HRef = string.Empty,
                NA = string.Empty,
                Screen = new ScreenInfoModel
                {
                    ColorDepth = 0,
                    Height = 0,
                    Width = 0
                },
                TimeZone = string.Empty,
                UserAgent = string.Empty,
                InvalidDeviceId = true
            };
        }

        private DeviceInfoModel TransformDeviceInfo(DeviceInfo deviceInfo)
        {
            var botD = new BotDModel();
            try
            {
                if (deviceInfo.BotD?.Result != null)
                {
                    botD = new BotDModel
                    {
                        Result = new BotDResultModel
                        {
                            Bot = deviceInfo.BotD.Result.Bot,
                            BotKind = deviceInfo.BotD.Result.BotKind
                        },
                        Detection = deviceInfo.BotD.Detection,
                        Component = deviceInfo.BotD.Component
                    };
                }
            }
            catch { }

            return new DeviceInfoModel
            {
                AppVersion = deviceInfo.AppVersion,
                CapturedDate = deviceInfo.CapturedDate,
                DeviceFPCode = deviceInfo.DeviceCode,
                DeviceNo = deviceInfo.DeviceId,
                HRef = deviceInfo.HRef,
                NA = deviceInfo.NA,
                TimeZone = deviceInfo.TimeZone,
                UserAgent = deviceInfo.UserAgent,
                Screen = deviceInfo.Screen == null
                    ? new ScreenInfoModel
                    {
                        ColorDepth = 0,
                        Height = 0,
                        Width = 0
                    }
                    : new ScreenInfoModel
                    {
                        ColorDepth = deviceInfo.Screen.ColorDepth,
                        Height = deviceInfo.Screen.Height,
                        Width = deviceInfo.Screen.Width
                    },
                BotD = botD
            };
        }

        public RawTransactionModel TransformRawTransaction(MessageQueue message)
        {
            var invalidDevice = string.Empty;
            var deviceCode = Utilities.GetDeviceCodeFromCapturedData(message.Device);
            var deviceInfo = GetDeviceInfo(deviceCode);
            var url = Utilities.GetURLFromDeviceCode(message.Device);
            var userAgent = Utilities.GetUserAgentFromDeviceCode(message.Device);
            var transBit = new BitArray(16);

            if (!string.IsNullOrWhiteSpace(message?.BotD))
            {
                deviceInfo.BotD = TransformBotD(message.BotD);
            }
            var flaggedCode = deviceInfo.BotD?.Result?.Bot == true ? deviceInfo.BotD.Result.BotKind : string.Empty;
            if (string.IsNullOrEmpty(flaggedCode))
            {
                var browserless = Browserless(userAgent, string.IsNullOrWhiteSpace(deviceInfo.DeviceNo));
                flaggedCode = browserless.ToString();
            }
            if (string.IsNullOrWhiteSpace(deviceInfo.DeviceNo) || deviceInfo.DeviceNo == "N/A")
            {
                deviceInfo.DeviceNo = null;
                transBit.Set((int)TransStatusEnum.DeviceCodeIsEmpty, true);
            }
            else
            {
                if (!Utilities.IsGuidDeviceId(deviceInfo.DeviceNo) || deviceInfo.InvalidDeviceId)
                {
                    invalidDevice = deviceCode;
                    deviceInfo.DeviceNo = null;
                    transBit.Set((int)TransStatusEnum.InvalidDI, true);
                }
            }

            if (ContainsUnicodeCharacter(message.Account))
            {
                transBit.Set((int)TransStatusEnum.InvalidLoginName, true);
            }

            var realIP = message.IP?.Trim();
            var ipConverted = realIP.Ip2Long();

            var botDModel = TransformBotdModel(message.BotD);
            var result = new RawTransactionModel
            {
                DeviceCode = deviceInfo.DeviceNo,
                InvalidDevice = string.IsNullOrWhiteSpace(invalidDevice) ? null : invalidDevice,
                TransStatus = Core.Utilities.ConvertBitArray2Int(transBit),
                LoginName = message.Account,
                SubscriberName = GetSubscriber(message.Subscriber),
                SubscriberId = message.SubscriberId,
                TransTime = Convert.ToDateTime(message.TransDate.ToString("yyyy-MM-dd HH:mm:ss.ffff")),
                IP = realIP,
                IPId = ipConverted.IpNumber,
                FakeIP = message.FakeIP?.Trim(),
                IsIPV6 = ipConverted.IsIpV6,
                PluginID = 0,
                URL = string.IsNullOrWhiteSpace(url) ? ConvertNA2Empty(SubStringUrl(deviceInfo.HRef)) : SubStringUrl(url),
                Action = message.Action,
                ActionResult = message.ActionResult,
                UserAgent = string.IsNullOrWhiteSpace(userAgent) ? ConvertNA2Empty(StripUnicodeCharactersFromString(deviceInfo.UserAgent)) : StripUnicodeCharactersFromString(userAgent),
                FingerprintCode = deviceInfo.DeviceFPCode != "N/A" ? deviceInfo.DeviceFPCode : null,
                FingerprintMoreInfo = deviceInfo.DeviceFPCode != "N/A" ? EncodeCapturedData(deviceInfo) : null,
                SessionKey = message.SessionKey,
                FlaggedCode = flaggedCode,
                BotDetection = GetBotDetectionTrue(botDModel),
                BotComponent = GetBotComponent(botDModel, flaggedCode)
            };

            return result;
        }


        private BotDModel TransformBotD(string encodedBotD)
        {
            var botD = Fanex.DI.Core.Encoders.Base64.Decode(encodedBotD);

            if (!string.IsNullOrEmpty(botD))
            {
                return JsonConvert.DeserializeObject<BotDModel>(botD);
            }

            return new BotDModel();
        }

        private static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }


        private string GetSubscriber(string subscriber)
        {
            //var extraSubs = AppSettings.ExtraSubs;
            //var extraSubsTest = AppSettings.ExtraSubsTest;
            //var isMatchingSubscriberTest = !string.IsNullOrEmpty(extraSubsTest) && subscriber == extraSubsTest;

            //if (AppSettings.IsReadyDbExtra && !string.IsNullOrWhiteSpace(extraSubs) && isMatchingSubscriberTest)
            //{
            //    var random = new Random();
            //    var subIndex = random.Next(0, extraSubsTest.Length);
            //    return extraSubs.Split(';')[subIndex];
            //}

            return subscriber;
        }

        private static string ConvertNA2Empty(string data)
        {
            return data != "N/A" ? data : null;
        }

        private static string StripUnicodeCharactersFromString(string inputValue)
        {
            return Regex.Replace(inputValue, @"[^\u0000-\u007F]", string.Empty);
        }

        private static string EncodeCapturedData(DeviceInfoModel deviceInfo)
        {
            return Cipher.Encrypt(string.Format("{0}#{1}#{2}#{3}#{4}",
                deviceInfo.Battery,
                deviceInfo.CapturedDate,
                deviceInfo.OSName,
                deviceInfo.TimeZone,
                deviceInfo.LocalIps));
        }

        private static string SubStringUrl(string url)
        {
            return url.IndexOf('?') != -1 ? url.Substring(0, url.IndexOf('?')) : url;
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
            var transaction = TransformRawTransaction(message);
            await GetTransactionAttribute(transaction);
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
        }

        private void HandleAddTransOfDCSSubs(RawTransactionModel tran)
        {
            transDcsSub.Add(tran);
            _dCSSubsTransactionFactory.HandleTransaction(transDcsSub);
            if (transDcsSub.Count >= appSettings.TransBatchSize)
            {
                //PushTransactionToMonitor(transDcsSub);
                transDcsSub.Clear();
            }
        }


        private BotDModel TransformBotdModel(string encodedBotD)
        {
            if (!string.IsNullOrEmpty(encodedBotD))
            {
                var botD = Fanex.DI.Core.Encoders.Base64.Decode(encodedBotD);

                if (!string.IsNullOrEmpty(botD))
                {
                    return JsonConvert.DeserializeObject<BotDModel>(botD);
                }
            }

            return new BotDModel();
        }

        private string GetFlaggedCode(BotDModel botDModel, string userAgent, bool emptyFP)
        {
            var flaggedCode = botDModel?.Result?.Bot == true ? botDModel.Result.BotKind : string.Empty;
            if (string.IsNullOrEmpty(flaggedCode))
            {
                var browserless = Browserless(userAgent, emptyFP);
                return browserless.ToString();
            }

            return flaggedCode;
        }

        private string GetBotDetectionTrue(BotDModel botDModel)
        {
            try
            {
                var detection = botDModel?.Detection;
                if (!string.IsNullOrEmpty(detection))
                {
                    JObject jObject = JObject.Parse(detection);
                    IEnumerable<string> filteredKeys = jObject.Properties()
                        .Where(p => p.Value.Value<bool>("bot"))
                        .Select(p => p.Name);

                    return string.Join(",", filteredKeys);
                }
            }
            catch { }

            return string.Empty;
        }

        private string GetBotComponent(BotDModel botDModel, string flaggedCode)
        {
            if (!appSettings.BotDBotComponentForHuman)
            {
                if (flaggedCode != Flagged.Human.ToString())
                {
                    return botDModel?.Component ?? string.Empty;
                }
            }
            else
            {
                return botDModel?.Component ?? string.Empty;
            }

            return string.Empty;
        }

        private Flagged Browserless(string userAgent, bool emptyFP)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(userAgent) && IsHeadless(userAgent))
                {
                    return Flagged.Headless;
                }

                if (emptyFP)
                {
                    return Flagged.EmptyDevice;
                }

                if (string.IsNullOrWhiteSpace(userAgent))
                {
                    return Flagged.EmptyUserAgent;
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message);
            }

            return Flagged.Human;
        }

        private bool IsHeadless(string userAgent)
        {
            if (userAgent.Contains("Headless")) return true;

            return false;
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
