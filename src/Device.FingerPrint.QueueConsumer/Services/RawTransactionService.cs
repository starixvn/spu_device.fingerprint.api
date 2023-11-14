namespace Device.FingerPrint.QueueConsumer.Services
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Core.Services;
    using Device.FingerPrint.Features.IPDB.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Redis;
    using Fanex.DeviceDetector.Factory;
    using Fanex.DeviceDetector.Models;
    using Fanex.Logging;
    using Fanex.Oneyed.Infrastructure.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public interface IRawTransactionService
    {

        UserAgentInfoModel ParseUserAgentInfo(string userAgent);

        FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel fpModel, HeaderModel headerModel, UserAgentInfoModel userAgentInfo);

        RawTransactionModel TransformRawTransaction(MessageQueue message);

        Task<RawTransactionModel> GetTransactionAttribute(RawTransactionModel model);

        Task ClearRedisAsync(IEnumerable<RawTransactionModel> transactions);

        void TransformTransactionByIps(IEnumerable<RawTransactionModel> transactions, bool isExtraSub);
    }

    public class RawTransactionService : IRawTransactionService
    {
        private readonly IRawTransactionAttributeService transactionAttributeService;
        private readonly ICrypto crypto;
        private readonly IAppSettings appSettings;
        private readonly IRedisService redisService;
        private readonly IIPDBServices ipdbServices;

        public RawTransactionService(
            ICrypto crypto,
            IAppSettings appSettings,
            IRedisService redisService,
            IRawTransactionAttributeService transactionAttributeService,
            IIPDBServices ipdbServices
            )
        {
            this.transactionAttributeService = transactionAttributeService;
            this.crypto = crypto;
            this.appSettings = appSettings;
            this.redisService = redisService;
            this.ipdbServices = ipdbServices;
        }

        public FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel fpModel, HeaderModel headerModel, UserAgentInfoModel userAgentInfo)
        {
            if (fpModel == null) return new FingerprintAttributeModel();

            var result = new FingerprintAttributeModel
            {
                FingerPrintCode = fpModel.Id,
                Attribute = fpModel.Attributes,
            };

            if (headerModel != null)
            {
                result.Accept = headerModel.Accept;
                result.Encoding = headerModel.Encoding;
                result.Headers = headerModel.Headers;

                result.UserAgent = userAgentInfo.UserAgent;
                result.BrowserName = userAgentInfo.ClientName;
                result.BrowserVersion = userAgentInfo.ClientVersion;
                result.OSName = userAgentInfo.OSName;
            }

            result.BrowserInstanceCode = HashBrowserInstanceCode(result);
            result.BrowserFamilyCode = HashBrowserFamilyCode(result);

            return result;
        }

        public UserAgentInfoModel ParseUserAgentInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return new UserAgentInfoModel();

            return transactionAttributeService.GetUserAgentInfo(userAgent);
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
                SubscriberName = message.Subscriber,
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

        public async Task<RawTransactionModel> GetTransactionAttribute(RawTransactionModel model)
        {
            if (model != null)
            {
                var hashKey = $"0:{model.SessionKey}";
                //var hashKey = $"{model.SubscriberId}:{model.SessionKey}";

                var fingerPrintTask = redisService.HashGetAsync<FingerprintModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint));
                var headerTask = redisService.HashGetAsync<HeaderModel>(hashKey, RedisString.GetRedisKeyName(RedisKey.Header));
                var cookieTask = redisService.HashGetAsync<string>(hashKey, RedisString.GetRedisKeyName(RedisKey.Cookie));

                await Task.WhenAll(fingerPrintTask, headerTask, cookieTask);

                var fingerPrint = fingerPrintTask.Result;
                var header = headerTask.Result;
                var cookie = cookieTask.Result;

                model.RawUserAgent = model.UserAgent;
                model.RawUserAgentInfo = ParseUserAgentInfo(model.UserAgent);

                model.FingerprintAttribute = TransformFingerprintAttributes(fingerPrint, header, model.RawUserAgentInfo);
                model.CookieCode = cookie;
            }

            return model;
        }

        public async Task ClearRedisAsync(IEnumerable<RawTransactionModel> transactions)
        {
            var groupByTrans = transactions.GroupBy(x => new
            {
                x.SessionKey
            });

            var tasks = new List<Task>();

            foreach (var trans in groupByTrans)
            {
                var task = redisService.HashDeleteAsync(trans.Key.SessionKey, RedisString.GetRedisKeyNames());
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        public void TransformTransactionByIps(IEnumerable<RawTransactionModel> transactions, bool isExtraSub)
        {
            if (isExtraSub)
            {
                var ipInfoList = GetIpInfos(transactions);
                if (ipInfoList != null && ipInfoList.Any())
                {
                    transactions = transactions.Select(x =>
                    {
                        var ipInfo = ipInfoList.FirstOrDefault(y => x.IPId == y.IP);

                        x.Country = ipInfo?.Country;
                        x.CountryCode = ipInfo?.CountryCode;
                        x.Region = ipInfo?.Region;
                        x.City = ipInfo?.City;
                        x.ISP = ipInfo?.ISP;
                        x.IPInfoCode = Cipher.Hashing2Int(string.Join(";", ipInfo?.CountryCode, ipInfo?.Region, ipInfo?.City, ipInfo?.ISP));

                        return x;
                    }).ToList();
                }
            }
        }

        private string HashBrowserInstanceCode(FingerprintAttributeModel model)
        {
            if (model == null) return string.Empty;

            var json = JsonConvert.SerializeObject(new BrowserInstanceModel
            {
                OSName = model.OSName,
                BrowserName = model.BrowserName,
                BrowserVersion = model.BrowserVersion,
                Accept = model.Accept,
                Encoding = model.Encoding,
                Headers = model.Headers,
                UserAgent = model.UserAgent,
                Platform = model.Platform
            });

            return crypto.GetMD5Hash(json);
        }

        private string HashBrowserFamilyCode(FingerprintAttributeModel model)
        {
            if (model == null) return string.Empty;

            var json = JsonConvert.SerializeObject(new BrowserFamilyModel
            {
                OSName = model.OSName,
                BrowserName = model.BrowserName,
                Platform = model.Platform
            });

            return crypto.GetMD5Hash(json);
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
            catch (Exception ex)
            {
                LogService.Error("Consumer TransformDeviceInfo: " + ex.Message + "\n" + JsonConvert.SerializeObject(deviceInfo));
            }

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

        private BotDModel TransformBotD(string encodedBotD)
        {
            var botD = Fanex.DI.Core.Encoders.Base64.Decode(encodedBotD);

            if (!string.IsNullOrEmpty(botD))
            {
                return JsonConvert.DeserializeObject<BotDModel>(botD);
            }

            return new BotDModel();
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
                LogService.Error("Consumer CheckBrowserless: " + ex.Message + "\n" + JsonConvert.SerializeObject(userAgent));
            }

            return Flagged.Human;
        }

        private bool IsHeadless(string userAgent)
        {
            if (userAgent.Contains("Headless")) return true;

            return false;
        }

        private static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
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

        private static string ConvertNA2Empty(string data)
        {
            return data != "N/A" ? data : null;
        }

        private static string SubStringUrl(string url)
        {
            return url.IndexOf('?') != -1 ? url.Substring(0, url.IndexOf('?')) : url;
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
            catch (Exception ex)
            {
                LogService.Error("Consumer GetBotDetectionTrue: " + ex.Message + "\n" + JsonConvert.SerializeObject(botDModel));
            }

            return string.Empty;
        }

        private string GetBotComponent(BotDModel botDModel, string flaggedCode)
        {
            if (!appSettings.GetValue<bool>("Consumer:TurnOnBotComponentForHuman"))
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

        private IEnumerable<IpInfoModel> GetIpInfos(IEnumerable<RawTransactionModel> transactions)
        {
            var ipAddressList = transactions.Where(x => x.IPId != -1 && !x.IsIPV6).Select(x => x.IPId).ToList();
            var ipAddressJoined = string.Join(",", ipAddressList);
            var ipInfoList = GetIpInfoList(ipAddressJoined);
            return ipInfoList;
        }

        private IEnumerable<IpInfoModel> GetIpInfoList(string ipAddressList)
        {
            return ipdbServices.GetIpInfoList(ipAddressList);
        }
    }
}
