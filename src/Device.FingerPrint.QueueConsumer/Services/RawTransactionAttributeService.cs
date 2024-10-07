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
using StackExchange.Redis;
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
        private const string _realTaggingPrefix = "RealTagging:";

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
            if (!appSettings.IsDisableJsChallenge && botDModel != null && !string.IsNullOrEmpty(botDModel.JscKey))
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
                        var fingerPrintEncodeTask = redisMultiplexService.HashGetWithDbIndexAsync(sessionKey.RedisDbIndex, sessionKey.SessionKey, RedisString.GetRedisKeyName(Model.RedisKey.FingerPrintEncode));
                        var headerTask = redisMultiplexService.HashGetWithDbIndexAsync(sessionKey.RedisDbIndex, sessionKey.SessionKey, RedisString.GetRedisKeyName(Model.RedisKey.Header));
                        var taggingTask = redisMultiplexService.EverTagging_GetValueFromRedis(_realTaggingPrefix + sessionKey.SessionKey);

                        await Task.WhenAll(fingerPrintEncodeTask, headerTask, taggingTask);

                        var fingerPrintEncode = fingerPrintEncodeTask.Result;
                        var header = headerTask.Result;
                        var realTagging = taggingTask.Result;

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
                            UpdateIPInfo(model, headerModel);
                        }

                        model.RawUserAgent = model.UserAgent;
                        model.UserAgentInfo = GetUserAgentInfo(model.UserAgent);

                        model.FingerprintAttribute = TransformFingerprintAttributes(model, fingerPrintModel, headerModel);

                        model.FPFingerPrintCode = model.FingerprintAttribute.FPFingerPrintCode;
                        model.FPFingerPrintExtraCode = model.FingerprintAttribute.FPFingerPrintExtraCode;
                        model.FPFingerPrintOldCode = model.FingerprintAttribute.FPFingerPrintOldCode;
                        model.FPUserAgentCode = crypto.GetMD5Hash(model.UserAgent);

                        model.FPTagging = GetTaggingValue(realTagging, sessionKey.SessionKey);

                        if (!string.IsNullOrEmpty(realTagging))
                        {
                            await redisMultiplexService.EverTagging_DeleteValueFromRedis(_realTaggingPrefix + sessionKey.SessionKey);
                        }

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

        private static void UpdateIPInfo(RawTransactionModel model, HeaderModel headerModel)
        {
            if (model == null || headerModel == null) return;

            var realIP = headerModel.RealIP?.Trim();
            var ipConverted = realIP.Ip2Long();

            model.IP = realIP;
            model.IPId = ipConverted?.IpNumber ?? 0;
            model.FakeIP = headerModel.FakeIP?.Trim();
            model.IsIPV6 = ipConverted?.IsIpV6 ?? false;
        }

        private static bool IsValidString(string decryptedText)
        {
            // Check if the string contains non-printable characters
            foreach (char c in decryptedText)
            {
                if (!char.IsLetterOrDigit(c) && !char.IsPunctuation(c) && !char.IsWhiteSpace(c))
                {
                    return false;
                }
            }
            return true;
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
                fingerPrintAttribute = ProcessForBrowserIgnoreDetectPrivateMode(fingerPrintAttribute, rawTransModel.UserAgentInfo);
                var newFingerprintModel = new FPNewFingerprintModel(fingerPrintAttribute, crypto);
                result.FPFingerPrintCode = newFingerprintModel.Code;
                result.FPFingerPrintAttribute = newFingerprintModel.AttributeModel;
            }

            if (HasData(oldFingerPrintAttribute))
            {
                var oldFingerprintModel = new FPOldFingerprintModel(oldFingerPrintAttribute, crypto);
                result.FPFingerPrintOldCode = oldFingerprintModel.Code;
            }

            if (HasData(fingerprintAttributeExtra))
            {
                var fingerprintExtraModel = new FPFingerprintExtraModel(fingerprintAttributeExtra, crypto);
                result.FPFingerPrintExtraCode = fingerprintExtraModel.Code;
                result.FPFingerPrintExtraAttribute = fingerprintExtraModel.AttributeModel;
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
                oldFingerPrintAttribute.UserAgent = userAgentInfo.UserAgent;
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

        private bool? GetIsIncognitoMode(FPFingerprintAttributeModel fingerprintAttribute)
        {
            if (string.IsNullOrEmpty(fingerprintAttribute?.IsIncognitoMode)) return null;

            return fingerprintAttribute.IsIncognitoMode == "true";
        }

        private string GetSuperCookies(FPFingerprintAttributeModel fingerprintAttributeModel)
        {
            var localStorage = fingerprintAttributeModel?.LocalStorage == "true";
            var sessionStorage = fingerprintAttributeModel?.SessionStorage == "true";
            var openDatabase = fingerprintAttributeModel?.OpenDatabase == "true";
            var indexedDb = fingerprintAttributeModel?.IndexedDB == "true";

            return localStorage + "|" + sessionStorage + "|" + openDatabase + "|" + indexedDb;
        }

        private FPFingerprintAttributeModel ProcessForBrowserIgnoreDetectPrivateMode(FPFingerprintAttributeModel fingerprintAttributeModel, UserAgentInfoModel userAgentInfo)
        {
            if (!string.IsNullOrEmpty(userAgentInfo?.ClientName))
            {
                if (IsIgnoreDetectPrivateMode(userAgentInfo.ClientName))
                {
                    fingerprintAttributeModel.IsIncognitoMode = "false";
                }
            }
            return fingerprintAttributeModel;
        }

        private bool IsIgnoreDetectPrivateMode(string browserName)
        {
            string _UCBROWSER = "uc browser";
            string _MIBROWSER = "miui browser";

            if (browserName.ToLower().Contains(_UCBROWSER)) return true;  //no need to detect incognito mode

            if (browserName.ToLower().Contains(_MIBROWSER)) return true;  //skip to detect incognito mode

            return false;
        }

        private static string GetTaggingValue(string realTagging, string sessionKey)
        {
            var taggingValue = string.IsNullOrEmpty(realTagging)
                ? sessionKey
                : Core.Utilities.RemoveRedisPrefix(realTagging, _realTaggingPrefix);

            var decryptedTagging = DeviceEncoder.Decrypt(taggingValue);
            return IsValidString(decryptedTagging) ? decryptedTagging : taggingValue;
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
