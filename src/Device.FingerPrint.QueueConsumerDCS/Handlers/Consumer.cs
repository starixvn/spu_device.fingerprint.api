using Device.FingerPrint.Queue;
using Device.FingerPrint.QueueConsumerDCS.Model;
using Device.FingerPrint.QueueConsumerDCS.Services;
using Device.FingerPrint.QueueConsumerDCS.Ultilities;
using EasyNetQ;
using EasyNetQ.Consumer;
using Fanex.Oneyed.Infrastructure.Models;
using Microsoft.Extensions.Hosting;
using Sentry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Device.FingerPrint.QueueConsumerDCS.Handlers
{
    public class Consumer : BackgroundService
    {
        private readonly IAppSettings appSettings;
        private readonly ISentryClient sentryClient;
        private readonly IFPCapturingService _fpcapturingService;

        public Consumer(IAppSettings appSettings, ISentryClient sentryClient, IFPCapturingService _fpcapturingService)
        {
            this.appSettings = appSettings;
            this.sentryClient = sentryClient;
            this._fpcapturingService = _fpcapturingService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartConsuming();

            return Task.CompletedTask;
        }

        private void StartConsuming()
        {
            try
            {
                if (appSettings.Configuration["ConsumerConfig:Queue-Consume"] == "1")
                {
                    var easyNetQConnector = RabbitMQBus();
                    var trans = new List<FPRawTransactionModel>();

                    easyNetQConnector.AdvanceBus.Consume(easyNetQConnector.Queue, x =>
                    {
                        x.Add<MessageQueue>(async (message, info) =>
                        {
                            try
                            {
                                var messageBody = message.Body;

                                var ignoreSubs = appSettings.Configuration["ConsumerConfig:IgnoreSubs"];

                                if (!string.IsNullOrWhiteSpace(ignoreSubs))
                                {
                                    var ignoreSubList = ignoreSubs.Split(';');

                                    if (!ignoreSubList.Any(x => x.Equals(messageBody.Subscriber, StringComparison.CurrentCultureIgnoreCase)))
                                    {
                                        var tran = TransformRawTransaction(messageBody);

                                        var url = appSettings.Configuration["ConsumerConfig:FPS-Web-API"] + "/getsubscriber/" + tran.SubscriberName;
                                        var result = await ApiHelper.Get<SubscriberModel>(url);
                                        if (result != null)
                                        {
                                            trans.Add(tran);

                                            if (trans.Count >= appSettings.TransBatch)
                                            {
                                                InsertTransaction(trans);

                                                trans.Clear();
                                            }
                                        }

                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                sentryClient.CaptureException(ex);
                            }
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }


        private void InsertTransaction(List<FPRawTransactionModel> trans)
        {
            try
            {
                _fpcapturingService.InsertTransaction(trans);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        private FPRawTransactionModel TransformRawTransaction(MessageQueue message)
        {
            //LogMessage(message);

            var invalidDevice = string.Empty;
            var deviceCode = Utilities.GetDeviceCodeFromCapturedData(message.Device);
            var deviceInfo = _fpcapturingService.GetDeviceInfo(deviceCode);
            var deviceNo = deviceInfo.DeviceNo;
            var url = Utilities.GetURLFromDeviceCode(message.Device);
            var userAgent = Utilities.GetUserAgentFromDeviceCode(message.Device);
            var transBit = new BitArray(16);
            var token = message.Token ?? string.Empty;
            var analysis = Utilities.GetAnalysis(message.Device);

            var flagged = Browserless(token, analysis, userAgent, deviceNo);

            if (string.IsNullOrWhiteSpace(deviceInfo.DeviceNo) || deviceInfo.DeviceNo == "N/A")
            {
                deviceInfo.DeviceNo = null;
                transBit.Set((int)TransStatus.DeviceCodeIsEmpty, true);
            }
            else
            {
                if (!Utilities.IsGuidDeviceId(deviceInfo.DeviceNo) || deviceInfo.InvalidDeviceId)
                {
                    invalidDevice = deviceCode;
                    deviceInfo.DeviceNo = null;
                    transBit.Set((int)TransStatus.InvalidDI, true);
                }
            }

            if (ContainsUnicodeCharacter(message.Account))
            {
                transBit.Set((int)TransStatus.InvalidLoginName, true);
            }

            var result = new FPRawTransactionModel
            {
                DeviceCode = deviceInfo.DeviceNo,
                InvalidDevice = string.IsNullOrWhiteSpace(invalidDevice) ? null : invalidDevice,
                TransStatus = Utilities.ConvertBitArray2Int(transBit),
                LoginName = message.Account,
                SubscriberName = message.Subscriber,
                SubscriberId = message.SubscriberId,
                TransTime = Convert.ToDateTime(message.TransDate.ToString("yyyy-MM-dd HH:mm:ss.ffff")),
                IP = message.IP,
                IPId = message.IP.Ip2Long().IpNumber,
                PluginID = 0,
                Flagged = (int)flagged,
                URL = string.IsNullOrWhiteSpace(url) ? ConvertNA2Empty(SubStringUrl(deviceInfo.HRef)) : SubStringUrl(url),
                Action = message.Action,
                ActionResult = message.ActionResult,
                UserAgent = string.IsNullOrWhiteSpace(userAgent) ? ConvertNA2Empty(StripUnicodeCharactersFromString(deviceInfo.UserAgent)) : StripUnicodeCharactersFromString(userAgent),
                FingerprintCode = deviceInfo.DeviceFPCode != "N/A" ? deviceInfo.DeviceFPCode : null,
                FingerprintMoreInfo = deviceInfo.DeviceFPCode != "N/A" ? EncodeCapturedData(deviceInfo) : null,
                TransKey = message.TransKey
            };

            return result;
        }

        private static string SubStringUrl(string url)
        {
            return url.IndexOf('?') != -1 ? url.Substring(0, url.IndexOf('?')) : url;
        }

        private static string ConvertNA2Empty(string data)
        {
            return data != "N/A" ? data : null;
        }

        private static String StripUnicodeCharactersFromString(string inputValue)
        {
            return Regex.Replace(inputValue, @"[^\u0000-\u007F]", String.Empty);
        }

        private static string EncodeCapturedData(FPDeviceInfoModel deviceInfo)
        {
            return Cipher.Encrypt(string.Format("{0}#{1}#{2}#{3}#{4}",
                deviceInfo.Battery,
                deviceInfo.CapturedDate,
                deviceInfo.OSName,
                deviceInfo.TimeZone,
                deviceInfo.LocalIps));
        }

        private static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }


        private Flagged Browserless(string token, string analysis, string userAgent, string deviceNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(deviceNo))
                {
                    return Flagged.EmptyDevice;
                }
                else
                {
                    if (deviceNo != "N/A" && Utilities.IsGuidDeviceId(deviceNo))
                    {
                        if (string.IsNullOrWhiteSpace(token))
                        {
                            return Flagged.EmptyToken;
                        }
                        else
                        {
                            if (!IsValidateToken(token))
                            {
                                return Flagged.InvalidToken;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(analysis))
                        {
                            return Flagged.EmptyAnalysis;
                        }
                        else
                        {
                            if (!IsValidAnalysisKey(analysis))
                            {
                                return Flagged.InvalidAnalysis;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(userAgent))
                        {
                            return Flagged.EmptyUserAgent;
                        }
                        else
                        {
                            if (IsHeadless(userAgent))
                            {
                                return Flagged.Headless;
                            }
                        }
                    }
                    else
                    {
                        return Flagged.InvalidDevice;
                    }
                }
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }

            return Flagged.Human;
        }

        private static bool IsValidateToken(string token)
        {
            var decryptedToken = BrowserlessDetection.Decrypt(token);
            var splitedToken = decryptedToken.Split('@');
            if (splitedToken.Length != 2)
            {
                return false;
            }
            else
            {
                if (splitedToken.Length > 0)
                {
                    var submitTime = DateTime.Parse(splitedToken[0]);

                    if (DateTime.Now.ToUniversalTime() < submitTime)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsValidAnalysisKey(string analysis)
        {
            string JsEnabledKey = "js-enabled";
            string[] AnalysisFormatKey = new string[] { "startTime", "version", "exceptions", "executions", "storages", "devices", "enable" };

            if (!analysis.Contains(JsEnabledKey))
            {
                foreach (var analysiItem in AnalysisFormatKey)
                {
                    if (!analysis.Contains(analysiItem))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsHeadless(string userAgent)
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
    }
}