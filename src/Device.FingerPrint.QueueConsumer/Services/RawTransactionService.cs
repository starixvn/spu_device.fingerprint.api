namespace Device.FingerPrint.QueueConsumer.Services
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Redis;
    using Fanex.Logging;
    using Newtonsoft.Json;
    using StackExchange.Redis;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IRawTransactionService
    {
        RawTransactionService TransformRawTransaction(MessageQueue message);

        void InsertOrUpdateEverTagging(string hashKey, IEnumerable<string> everTaggings, long expireSecond);

        void InsertRawTransactionDetail(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra);

    }

    public class RawTransactionService : IRawTransactionService
    {
        private readonly IRawTransactionAttributeService rawTransactionAttributeService;
        private readonly IDeviceService deviceService;
        private readonly IBotDetectionService botDetectionService;
        private readonly ITransactionService transactionService;
        private readonly IAppSettings appSettings;
        private readonly IDatabase everTaggingRedisDB;

        public RawTransactionModel TransformRawTransModel { get; private set; }

        public RawTransactionService(
            IRedisMultiplexDbService redisMultiplexService,
            IRawTransactionAttributeService rawTransactionAttributeService,
            IDeviceService deviceService,
            IBotDetectionService botDetectionService,
            ITransactionService transactionService,
            IAppSettings appSettings
        )
        {
            this.rawTransactionAttributeService = rawTransactionAttributeService;
            this.deviceService = deviceService;
            this.botDetectionService = botDetectionService;
            this.transactionService = transactionService;
            this.appSettings = appSettings;

            this.everTaggingRedisDB = redisMultiplexService.GetDatabase(appSettings.EverTaggingDBIndex);
        }

        public RawTransactionService TransformRawTransaction(MessageQueue message)
        {
            try
            {
                var invalidDevice = string.Empty;
                var deviceCode = string.Empty;
                var url = string.Empty;
                var userAgent = string.Empty;
                var deviceInfo = new DeviceInfoModel();

                if (message.Device != null)
                {
                    deviceCode = message.Device.Di;
                    deviceInfo = deviceService.GetDeviceInfo(deviceCode, message);
                    url = message.Device.Url;
                    userAgent = message.Device.UserAgent;
                }

                if(message.Mobile != null)
                {
                    userAgent = string.IsNullOrEmpty(userAgent) ? message.Mobile.UserAgent : userAgent;
                }

                var transBit = new BitArray(16);
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

                var botDModel = botDetectionService.TransformBotdModel(message);

                var flaggedCode = botDetectionService.GetFlaggedCode(botDModel, deviceInfo.DeviceNo, userAgent);
                var botDetection = botDModel?.IsVer3 == true ? botDModel?.Detection : botDetectionService.GetBotDetectionTrue(botDModel);

                var realIP = message.IP?.Trim();
                var ipConverted = realIP.Ip2Long();

                var rawTransModel = new RawTransactionModel
                {
                    DeviceCode = deviceInfo.DeviceNo,
                    InvalidDevice = string.IsNullOrWhiteSpace(invalidDevice) ? null : invalidDevice,
                    TransStatus = Core.Utilities.ConvertBitArray2Int(transBit),
                    LoginName = message.Account,
                    SubscriberName = message.Subscriber,
                    SubscriberId = message.SubscriberId,
                    TransTime = Convert.ToDateTime(message.TransDate.ToString("yyyy-MM-dd HH:mm:ss.ffff")),
                    PluginID = 0,
                    URL = string.IsNullOrWhiteSpace(url) ? Utilities.ConvertNA2Empty(Utilities.SubStringUrl(deviceInfo.HRef)) : Utilities.SubStringUrl(url),
                    Action = message.Action,
                    ActionResult = message.ActionResult,
                    UserAgent = string.IsNullOrWhiteSpace(userAgent) ? Utilities.ConvertNA2Empty(Utilities.StripUnicodeCharactersFromString(deviceInfo.UserAgent)) : Utilities.StripUnicodeCharactersFromString(userAgent),
                    FingerprintCode = deviceInfo.DeviceFPCode != "N/A" ? deviceInfo.DeviceFPCode : null,
                    FingerprintMoreInfo = deviceInfo.DeviceFPCode != "N/A" ? Utilities.EncodeCapturedData(deviceInfo) : null,
                    SessionKey = message.SessionKey,
                    FlaggedCode = flaggedCode,
                    BotDetection = botDetection,
                    BotComponent = botDModel?.Component ?? string.Empty,
                    IP = realIP,
                    IPId = ipConverted.IpNumber,
                    FakeIP = message.FakeIP?.Trim(),
                    IsIPV6 = ipConverted.IsIpV6,
                    ScriptVersion = deviceInfo.Version
                };

                if(message.Mobile != null)
                {
                    rawTransModel.MBAndroidCode = message.Mobile.AndroidCode;
                    rawTransModel.MBCustomCode = message.Mobile.CustomCode;
                    rawTransModel.MBMediaDRMCode = message.Mobile.MediaDRMCode;
                    rawTransModel.MBIsMobileApp = message.Mobile.IsMobileAppTrans;
                    rawTransModel.MBIsEmulator = message.Mobile.IsEmulator;
                    rawTransModel.MBModelName = message.Mobile.ModelName;
                    rawTransModel.MBManufacturer = message.Mobile.Manufacturer;
                    rawTransModel.MBBrand = message.Mobile.Brand;
                    rawTransModel.MBOS = message.Mobile.OS;
                    rawTransModel.MBTimezone = message.Mobile.Timezone;
                    rawTransModel.MBLanguage = message.Mobile.SystemLanguage;
                    rawTransModel.MBCountry = message.Mobile.Country;
                    rawTransModel.MBSDKVersion = message.Mobile.SdkVersion;
                }
                TransformRawTransModel = rawTransModel;

                return this;
            }
            catch (Exception ex)
            {
                _ = LogService.ErrorAsync($"{appSettings.Environment} - TransformRawTransaction has error:\n {ex.Message}");

                return null;
            }
        }

        public RawTransactionModel GetRawTransAttributes()
        {
            return rawTransactionAttributeService.GetTransactionAttribute(TransformRawTransModel);
        }

        public void InsertOrUpdateEverTagging(string hashKey, IEnumerable<string> everTaggings, long expireSeconds)
        {
            try
            {
                foreach (var code in everTaggings)
                {
                    var expiry = TimeSpan.FromSeconds(expireSeconds);

                    everTaggingRedisDB.StringSet(
                        key: $"{hashKey}:{code}",
                        value: DateTime.Now.ToString(),
                        expiry: expiry,
                        keepTtl: false,
                        when: When.Always,
                        flags: CommandFlags.FireAndForget
                    );
                }
            }
            catch (Exception ex)
            {
                _ = QueueConsumer.Utilities.LogErrorAsync(ex);
            }
        }

        public void InsertRawTransactionDetail(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            Parallel.Invoke(
                () => InsertFPFingerPrint(transactions, isTransOfDBExtra),
                () => InsertFPUserAgent(transactions, isTransOfDBExtra)
            );
        }

        private void InsertFPUserAgent(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            try
            {
                var userAgentInfos = GetValidInfo(transactions, t => t.UserAgentInfo);
                if (userAgentInfos != null && userAgentInfos.Any())
                {
                    var userAgentJson = JsonConvert.SerializeObject(userAgentInfos);
                    transactionService.InsertFPUserAgent(userAgentJson, isTransOfDBExtra);
                }
            }
            catch (Exception ex)
            {
                _ = QueueConsumer.Utilities.LogErrorAsync(ex);
            }
        }

        private void InsertFPFingerPrint(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            try
            {
                var fingerprintAttribute = GetValidInfo(transactions, t => t.FingerprintAttribute);
                if (fingerprintAttribute != null && fingerprintAttribute.Any())
                {
                    var fingerPrintJson = JsonConvert.SerializeObject(fingerprintAttribute);
                    transactionService.InsertFPFingerPrint(fingerPrintJson, isTransOfDBExtra);
                }
            }
            catch (Exception ex)
            {
                _ = QueueConsumer.Utilities.LogErrorAsync(ex);
            }
        }

        private static IEnumerable<TResult> GetValidInfo<TResult>(IEnumerable<RawTransactionModel> transactions, Func<RawTransactionModel, TResult> selector)
        {
            return transactions
                .Select(selector)
                .Where(result => result != null)
                .Distinct();
        }

    }
}
