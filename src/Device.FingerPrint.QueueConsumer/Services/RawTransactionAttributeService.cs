using Device.FingerPrint.Core;
using Device.FingerPrint.Core.Encoders;
using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Features.LogInfo.Model;
using Device.FingerPrint.Features.LogInfo.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.Model.DTO;
using Device.FingerPrint.Model.Enums;
using Device.FingerPrint.QueueConsumer.Models;
using Device.FingerPrint.Redis;
using DeviceDetectorNET;
using Fanex.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface IRawTransactionAttributeService
    {
        Task GetTransactionAttribute(RawTransactionModel model, BotDModel botDModel);
    }

    public class RawTransactionAttributeService : IRawTransactionAttributeService
    {
        private readonly ICrypto crypto;
        private readonly IRedisService redisService;
        private readonly IRedisMultiplexService redisMultiplexService;
        private readonly ILogInfoServices logInfoServices;
        private readonly IAppSettings appSettings;
        private const string base64Key = "wFKfVDGZqRbyYCBi6IveMaQAh1ntWcP8ujT/U7r2xXs+J0L=4d3zEHSo5glN9kOpm";

        public RawTransactionAttributeService(
            ICrypto crypto,
            IRedisService redisService,
            IRedisMultiplexService redisMultiplexService,
            ILogInfoServices logInfoServices,
            IAppSettings appSettings)
        {
            this.crypto = crypto;
            this.redisService = redisService;
            this.redisMultiplexService = redisMultiplexService;
            this.logInfoServices = logInfoServices;
            this.appSettings = appSettings;
        }

        public async Task GetTransactionAttribute(RawTransactionModel model, BotDModel botDModel)
        {
            if (botDModel != null && !string.IsNullOrEmpty(botDModel.JscKey))
            {
                var jscKey = botDModel.JscKey;
                model.ChallengeCode = jscKey;

                var jsChallengeTask = redisService.RedisJSChallengeDatabase.GetAsync<string>($"Challenge:{jscKey}");
                var jsChallenge = jsChallengeTask.Result;
                TransformJSChallengeInfo(model, jscKey, jsChallenge);
            }

            if (model != null && !string.IsNullOrEmpty(model.SessionKey))
            {
                if (!model.SessionKey.Contains(':'))
                {
                    _ = LogService.InfoAsync($"{appSettings.Environment} - SessionKey does not contain a colon:\n {JsonConvert.SerializeObject(model)}");
                }
                else
                {
                    var sessionKey = Core.Utilities.ParseSessionKey(model.SessionKey, appSettings.RedisDatabase);

                    if (sessionKey != null && sessionKey.RedisDbIndex > 0)
                    {
                        var fingerPrintEncodeTask = redisMultiplexService.HashGetWithDbIndexAsync(sessionKey.RedisDbIndex, sessionKey.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrintEncode));
                        var headerTask = redisMultiplexService.HashGetWithDbIndexAsync(sessionKey.RedisDbIndex, sessionKey.SessionKey, RedisString.GetRedisKeyName(RedisKey.Header));

                        await Task.WhenAll(fingerPrintEncodeTask, headerTask);

                        var fingerPrintEncode = fingerPrintEncodeTask.Result;
                        var header = headerTask.Result;

                        var fingerPrintModel = new FingerprintModel();
                        if (!string.IsNullOrEmpty(fingerPrintEncode))
                        {
                            var decodeData = Base64.Decode(fingerPrintEncode, base64Key);
                            fingerPrintModel = JsonConvert.DeserializeObject<FingerprintModel>(decodeData);
                        }

                        var headerModel = new HeaderModel();
                        if (!string.IsNullOrEmpty(header))
                        {
                            headerModel = JsonConvert.DeserializeObject<HeaderModel>(header);
                        }

                        model.RawUserAgent = model.UserAgent;
                        model.UserAgentInfo = GetUserAgentInfo(model.UserAgent);

                        model.FingerprintAttribute = TransformFingerprintAttributes(model, fingerPrintModel, headerModel);

                        model.FPTagging = DeviceEncoder.Decrypt(sessionKey.SessionKey);
                        model.FPFingerPrintCode = model.FingerprintAttribute.FPFingerPrintCode;
                        model.FPFingerPrintExtraCode = model.FingerprintAttribute.FPFingerPrintExtraCode;
                        model.FPFingerPrintOldCode = model.FingerprintAttribute.FPFingerPrintOldCode;
                        model.FPUserAgentCode = crypto.GetMD5Hash(model.UserAgent);

                        model.IsIncognitoMode = GetIsIncognitoMode(model.FingerprintAttribute.FPFingerPrintAttribute);

                        // Log Info Consumer
                        var subscriberName = model.SubscriberName;
                        if (appSettings.GetValue<bool>("Consumer:LogInfo")
                            && !subscriberName.Contains("Alpha", StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.IsNullOrEmpty(sessionKey.SessionKey))
                            {
                                LogSessionKeyEmpty(model);
                            }
                            else if (fingerPrintEncode == null && header == null)
                            {
                                LogRedisValueEmpty(model);
                            }
                        }
                    }
                    else
                    {
                        _ = LogService.ErrorAsync($"{appSettings.Environment} - SessionKey cannot parse:\n {JsonConvert.SerializeObject(model)}");
                    }
                }
            }
        }

        private InsertFPFingerprintEntity TransformFingerprintAttributes(RawTransactionModel rawTransModel, FingerprintModel fpModel, HeaderModel headerModel)
        {
            var result = new InsertFPFingerprintEntity();

            if (fpModel == null || string.IsNullOrEmpty(fpModel.Attributes))
                return result;

            var attributesDictionary = DeserializeToDictionary(fpModel.Attributes);

            var fingerPrintAttribute = JsonConvert.DeserializeObject<FPFingerprintAttributeModel>(JsonConvert.SerializeObject(attributesDictionary));
            var oldFingerPrintAttribute = TransformOldFingerprintAttribute(attributesDictionary, fingerPrintAttribute, rawTransModel);
            var fingerprintAttributeExtra = TransformFingerprintAttributeExtra(fpModel, headerModel, rawTransModel, oldFingerPrintAttribute);

            if (HasData(fingerPrintAttribute))
            {
                result.FPFingerPrintCode = GetMD5HashFromObject(fingerPrintAttribute);
                result.FPFingerPrintAttribute = fingerPrintAttribute;
            }

            if (HasData(oldFingerPrintAttribute))
            {
                result.FPFingerPrintOldCode = GetMD5HashFromObject(oldFingerPrintAttribute);
            }

            if (HasData(fingerprintAttributeExtra))
            {
                result.FPFingerPrintExtraCode = GetMD5HashFromObject(fingerprintAttributeExtra);
                result.FPFingerPrintExtraAttribute = fingerprintAttributeExtra;
            }

            return result;
        }

        private FPNewFingerprintModel TransformNewFingerprintJS(FingerprintModel fpModel)
        {
            FPNewFingerprintModel result = null;
            var hasFPFingerPrint = false;

            if (fpModel != null && !string.IsNullOrEmpty(fpModel.Attributes))
            {
                var stringDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(fpModel.Attributes)
                    .ToDictionary(kvp => kvp.Key, kvp => JsonConvert.SerializeObject(kvp.Value));

                var fingerPrintAttribute = new FPFingerprintAttributeModel();
                foreach (var property in typeof(FPFingerprintAttributeModel).GetProperties())
                {
                    var camelCaseName = Char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
                    if (stringDictionary.TryGetValue(camelCaseName, out string value))
                    {
                        property.SetValue(fingerPrintAttribute, value);
                        hasFPFingerPrint = true;
                    }
                }

                if (hasFPFingerPrint)
                {
                    result = new FPNewFingerprintModel(fingerPrintAttribute, crypto);
                }

            }

            return result;
        }

        private static void TransformJSChallengeInfo(RawTransactionModel model, string jscKey, string jsChallenge)
        {
            if (!string.IsNullOrEmpty(jsChallenge))
            {
                var jsChallengeInfo = JsonConvert.DeserializeObject<JSChallengeInfo>(jsChallenge);
                if (string.IsNullOrEmpty(jsChallengeInfo.Expect))
                {
                    jsChallengeInfo.Expect = jsChallengeInfo.Expected;
                }

                if (jsChallengeInfo.Expect != jsChallengeInfo.Actual && jsChallengeInfo.Result == 0)
                {
                    jsChallengeInfo.Result = 1;
                }
                jsChallengeInfo.ChallengeCode = jscKey;
                model.RawJsChallengeInfo = jsChallengeInfo;

                model.ChallengeCode = jscKey;
            }
        }

        private UserAgentInfoModel GetUserAgentInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return new UserAgentInfoModel();

            var deviceDetector = new DeviceDetector(userAgent);

            deviceDetector.SkipBotDetection();
            deviceDetector.Parse();

            var userAgentInfo = new UserAgentInfoModel { UserAgent = userAgent };

            var clientInfo = deviceDetector.GetClient();

            if (clientInfo != null && clientInfo.Success)
            {
                userAgentInfo.ClientType = clientInfo.Match.Type;
                userAgentInfo.ClientName = clientInfo.Match.Name;
                userAgentInfo.ClientVersion = clientInfo.Match.Version;
            }

            var osInfo = deviceDetector.GetOs();

            if (osInfo != null && osInfo.Success)
            {
                userAgentInfo.OSName = osInfo.Match.Name;
                userAgentInfo.OSShortName = osInfo.Match.ShortName;
                userAgentInfo.OSVersion = osInfo.Match.Version;
                userAgentInfo.OSPlatform = osInfo.Match.Platform;
            }

            userAgentInfo.Device = deviceDetector.GetDeviceName();
            userAgentInfo.Brand = deviceDetector.GetBrandName();
            userAgentInfo.Model = deviceDetector.GetModel();
            userAgentInfo.DeviceType = GetDeviceType(deviceDetector);

            userAgentInfo.FPUserAgentCode = crypto.GetMD5Hash(userAgent);

            return userAgentInfo;
        }


        private Dictionary<string, string> DeserializeToDictionary(string attributesJson)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(attributesJson)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value == null ? null : JsonConvert.SerializeObject(kvp.Value));
        }

        private FPOldFingerprintAttributeModel TransformOldFingerprintAttribute(Dictionary<string, string> attributesDictionary, FPFingerprintAttributeModel fingerPrintAttribute, RawTransactionModel rawTransModel)
        {
            var oldFingerPrintAttribute = JsonConvert.DeserializeObject<FPOldFingerprintAttributeModel>(JsonConvert.SerializeObject(attributesDictionary));

            oldFingerPrintAttribute.SuperCookies = GetSuperCookies(fingerPrintAttribute);

            var userAgentInfo = rawTransModel.UserAgentInfo;
            if (userAgentInfo != null)
            {
                oldFingerPrintAttribute.Useragent = userAgentInfo.UserAgent;
                oldFingerPrintAttribute.Browser = userAgentInfo.ClientName + " " + userAgentInfo.ClientVersion;
                oldFingerPrintAttribute.DeviceType = userAgentInfo.DeviceType;
            }

            return oldFingerPrintAttribute;
        }

        private FPFingerprintAttributeExtraModel TransformFingerprintAttributeExtra(FingerprintModel fpModel, HeaderModel headerModel, RawTransactionModel rawTransModel, FPOldFingerprintAttributeModel oldFingerPrintAttribute)
        {
            var fingerprintAttributeExtra = new FPFingerprintAttributeExtraModel
            {
                Platform = fpModel.Platform,
                Accept = headerModel?.Accept,
                Encoding = headerModel?.Encoding,
                Headers = headerModel?.Headers,
                UserAgent = rawTransModel.UserAgentInfo?.UserAgent,
                BrowserName = rawTransModel.UserAgentInfo?.ClientName,
                BrowserVersion = rawTransModel.UserAgentInfo?.ClientVersion,
                OSName = rawTransModel.UserAgentInfo?.OSName,
                Navigator = oldFingerPrintAttribute.Navigator,
                TimezoneOffset = oldFingerPrintAttribute.TimezoneOffset,
                DeviceType = oldFingerPrintAttribute.DeviceType
            };

            return fingerprintAttributeExtra;
        }

        private string GetMD5HashFromObject(object obj)
        {
            return crypto.GetMD5Hash(JsonConvert.SerializeObject(obj));
        }

        private bool HasData<T>(T model)
        {
            var properties = model.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(model);

                if (value != null && !(value is string str && string.IsNullOrEmpty(str)))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetDeviceType(DeviceDetector deviceDetector)
        {
            int _OTHERTYPE = 0;
            int _DESTOPTYPE = 1;
            int _TABLETTYPE = 2;
            int _MOBILETYPE = 3;

            if (deviceDetector.IsDesktop()) return _DESTOPTYPE;

            if (deviceDetector.IsTablet()) return _TABLETTYPE;

            if (deviceDetector.IsMobile()) return _MOBILETYPE;

            return _OTHERTYPE;
        }

        private bool GetIsIncognitoMode(FPFingerprintAttributeModel fingerprintAttribute)
        {
            bool.TryParse(fingerprintAttribute.IsIncognitoMode, out bool isIncognitoMode);
            return isIncognitoMode;
        }

        private string GetSuperCookies(FPFingerprintAttributeModel fingerprintAttributeModel)
        {
            var localStorage = fingerprintAttributeModel.LocalStorage == "true" ? '1' : '0';
            var sessionStorage = fingerprintAttributeModel.SessionStorage == "true" ? '1' : '0';
            var openDatabase = fingerprintAttributeModel.OpenDatabase == "true" ? '1' : '0';
            var indexedDb = fingerprintAttributeModel.IndexedDB == "true" ? '1' : '0';

            return localStorage + "" + sessionStorage + openDatabase + indexedDb;
        }

        #region Log Info
        private void LogSessionKeyEmpty(RawTransactionModel model)
        {
            var logType = (int)LogType.SessionKeyEmpty;
            if (model.SessionKey == null)
            {
                logType = (int)LogType.SessionKeyNull;
            }

            var logInfoModel = TransformToLogInfoModel(model, logType);
            logInfoServices.InsertDCSConsumerLogAsync(logInfoModel);
        }

        private void LogRedisValueEmpty(RawTransactionModel model)
        {
            var logInfoModel = TransformToLogInfoModel(model, (int)LogType.RedisValueEmpty);
            logInfoServices.InsertDCSConsumerLogAsync(logInfoModel);
        }

        private LogInfoModel TransformToLogInfoModel(RawTransactionModel model, int logType)
        {
            return new LogInfoModel
            {
                LogType = logType,
                SessionKey = $"{appSettings.Environment} - {model.SessionKey}",
                LoginName = model.LoginName,
                SubscriberName = model.SubscriberName,
                TransTime = model.TransTime,
                URL = model.URL,
                UserAgent = model.UserAgent
            };
        }

        #endregion    
    }

}
