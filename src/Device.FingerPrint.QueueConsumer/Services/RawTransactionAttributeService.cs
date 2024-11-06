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
using System.Collections;
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
        private readonly IRedisMultiplexDbService redisMultiplexService;
        private readonly ILogInfoServices logInfoServices;
        private readonly IAppSettings appSettings;
        private readonly IDatabase everTaggingRedisDB;

        private const string base64Key = "wFKfVDGZqRbyYCBi6IveMaQAh1ntWcP8ujT/U7r2xXs+J0L=4d3zEHSo5glN9kOpm";
        private const string _realTaggingPrefix = "RealTagging:";
        private readonly List<string> fieldKeys = new();

        public RawTransactionAttributeService(
            ICrypto crypto,
            IRedisMultiplexDbService redisMultiplexService,
            ILogInfoServices logInfoServices,
            IAppSettings appSettings)
        {
            this.crypto = crypto;
            this.redisMultiplexService = redisMultiplexService;
            this.logInfoServices = logInfoServices;
            this.appSettings = appSettings;

            everTaggingRedisDB = redisMultiplexService.GetDatabase(appSettings.EverTaggingDBIndex);

            fieldKeys = new List<string>
                            {
                                RedisString.GetRedisKeyName(Model.RedisKey.FingerPrintEncode),
                                RedisString.GetRedisKeyName(Model.RedisKey.Header)
                            };
        }

        public async Task GetTransactionAttribute(RawTransactionModel model, BotDModel botDModel)
        {
            try
            {
                var isLogged = false;

                if (model != null)
                {
                    model.RawUserAgent = model.UserAgent;
                    model.UserAgentInfo = GetUserAgentInfo(model.UserAgent);
                    model.FPUserAgentCode = crypto.GetMD5Hash(model.UserAgent);

                    if (!string.IsNullOrEmpty(model.SessionKey))
                    {
                        if (!model.SessionKey.Contains(':'))
                        {
                            _ = LogConsumerError(model, LogType.SessionKeyColon);
                            isLogged = true;
                        }

                        var sessionKey = Core.Utilities.ParseSessionKey(model.SessionKey, appSettings.RedisDatabase);
                        if (sessionKey != null)
                        {
                            var redisValues = await redisMultiplexService.HashGetWithDbIndexAsync(sessionKey.RedisDbIndex, sessionKey.SessionKey, fieldKeys);

                            var fingerPrintEncode = redisValues[0];
                            var header = redisValues[1];

                            var realTagging = await everTaggingRedisDB.StringGetAsync(_realTaggingPrefix + sessionKey.SessionKey);

                            var fingerPrintModel = new FingerprintModel();
                            if (!string.IsNullOrEmpty(fingerPrintEncode) && Utilities.IsBase64String(fingerPrintEncode))
                            {
                                try
                                {
                                    var decodeData = Base64.Decode(fingerPrintEncode, base64Key);
                                    if (Utilities.IsValidJson(decodeData))
                                    {
                                        fingerPrintModel = JsonConvert.DeserializeObject<FingerprintModel>(decodeData);
                                    }
                                }
                                catch (FormatException ex)
                                {
                                    _ = LogService.InfoAsync($"{appSettings.Environment} - FormatException has error:\nfingerPrintEncode: {fingerPrintEncode}\n{ex.Message}");
                                }
                                catch (JsonException ex)
                                {
                                    _ = LogService.InfoAsync($"{appSettings.Environment} - JsonException has error:\nfingerPrintEncode: {fingerPrintEncode}\n{ex.Message}");
                                }
                            }

                            var headerModel = new HeaderModel();
                            if (!string.IsNullOrEmpty(header) && Utilities.IsValidJson(header))
                            {
                                try
                                {
                                    headerModel = JsonConvert.DeserializeObject<HeaderModel>(header);
                                    UpdateIPInfo(model, headerModel);
                                }
                                catch (JsonException ex)
                                {
                                    _ = LogService.InfoAsync($"{appSettings.Environment} - JsonException header has error:\nheader: {header}\n {ex.Message}");
                                }
                            }

                            model.FingerprintAttribute = TransformFingerprintAttributes(model, fingerPrintModel, headerModel);

                            model.FPFingerPrintCode = model.FingerprintAttribute.FPFingerPrintCode;
                            model.FPFingerPrintExtraCode = model.FingerprintAttribute.FPFingerPrintExtraCode;
                            model.FPFingerPrintOldCode = model.FingerprintAttribute.FPFingerPrintOldCode;

                            var taggingValue = GetTaggingValue(realTagging, sessionKey.SessionKey);
                            model.FPTagging = taggingValue;

                            // Log when FPTagging is wrong
                            if (!string.IsNullOrEmpty(taggingValue) && taggingValue.Length != 32)
                            {
                                _ = LogConsumerError(model, LogType.TaggingLength);
                            }

                            if (!string.IsNullOrEmpty(realTagging))
                            {
                                _ = everTaggingRedisDB.KeyDeleteAsync(_realTaggingPrefix + sessionKey.SessionKey);
                            }

                            model.IsIncognitoMode = GetIsIncognitoMode(model.FingerprintAttribute.FPFingerPrintAttribute);

                        }
                        else if (!isLogged)
                        {
                            _ = LogConsumerError(model, LogType.SessionKeyParse);
                        }
                    }
                    else if (model.SessionKey == null)
                    {
                        _ = LogConsumerError(model, LogType.SessionKeyNull);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = LogService.InfoAsync($"{appSettings.Environment} - GetTransactionAttribute has error:\n {ex.Message}");
            }
        }

        private static void UpdateIPInfo(RawTransactionModel model, HeaderModel headerModel)
        {
            if (model == null || headerModel == null) return;

            var realIP = headerModel.RealIP?.Trim();

            if (!string.IsNullOrEmpty(realIP))
            {
                var ipConverted = realIP.Ip2Long();

                model.IP = realIP;
                model.IPId = ipConverted?.IpNumber ?? 0;
                model.IsIPV6 = ipConverted?.IsIpV6 ?? false;

                if (!string.IsNullOrEmpty(headerModel.FakeIP))
                {
                    model.FakeIP = headerModel.FakeIP?.Trim();
                }
            }
        }

        private InsertFPFingerprintEntity TransformFingerprintAttributes(RawTransactionModel rawTransModel, FingerprintModel fpModel, HeaderModel headerModel)
        {
            var result = new InsertFPFingerprintEntity();

            if (fpModel == null || string.IsNullOrEmpty(fpModel.Attributes))
                return result;

            var attributesDictionary = DeserializeToDictionary(fpModel.Attributes);

            if (attributesDictionary == null || !attributesDictionary.Any())
            {
                return result;
            }

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

        private static Dictionary<string, string> DeserializeToDictionary(string attributesJson)
        {
            if (string.IsNullOrEmpty(attributesJson) || !Utilities.IsValidJson(attributesJson))
            {
                return new Dictionary<string, string>();
            }

            try
            {
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(attributesJson);

                if (dictionary == null || !dictionary.Any())
                {
                    return new Dictionary<string, string>();
                }

                return dictionary.ToDictionary(kvp => kvp.Key,
                                                kvp => kvp.Value == null
                                                    ? null
                                                    : (kvp.Value is string
                                                        ? kvp.Value.ToString()
                                                        : JsonConvert.SerializeObject(kvp.Value))
                                            );
            }
            catch (JsonSerializationException ex)
            {
                _ = Utilities.LogInfoAsync(ex);
            }
            catch (Exception ex)
            {
                _ = Utilities.LogInfoAsync(ex);
            };

            return new Dictionary<string, string>();
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
            if (!string.IsNullOrEmpty(realTagging) && IsHexString(realTagging))
            {
                try
                {
                    var decryptedStr = DeviceEncoder.Decrypt(realTagging);
                    if (CheckGuid(decryptedStr))
                    {
                        return decryptedStr;
                    }
                }
                catch
                {
                }
            }

            if (!string.IsNullOrEmpty(sessionKey) && IsHexString(sessionKey))
            {
                try
                {
                    var decryptedStr1 = DeviceEncoder.Decrypt(sessionKey);
                    if (CheckGuid(decryptedStr1))
                    {
                        return decryptedStr1;
                    }
                }
                catch
                {
                }
            }

            return string.Empty;
        }

        private static bool IsHexString(string hex)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(hex, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        private static bool CheckGuid(string decryptedText)
        {
            if (decryptedText.Length != 32)
            {
                return false;
            }

            return Guid.TryParse(decryptedText, out _);
        }

        #region Log Info
        private async Task LogConsumerError(RawTransactionModel model, LogType logType)
        {
            if (appSettings.GetValue<bool>("Consumer:LogInfo"))
            {
                var logInfoModel = TransformToLogInfoModel(model, (int)logType);
                await logInfoServices.InsertDCSConsumerLogAsync(logInfoModel);
            }
        }

        private LogInfoModel TransformToLogInfoModel(RawTransactionModel model, int logType)
        {
            var sessionKey = model.SessionKey ?? string.Empty;
            var tagging = model.FPTagging ?? string.Empty;

            return new LogInfoModel
            {
                LogType = logType,
                Environment = appSettings.Environment,
                ScriptVersion = model.ScriptVersion,
                SessionKey = sessionKey,
                Tagging = tagging,
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
