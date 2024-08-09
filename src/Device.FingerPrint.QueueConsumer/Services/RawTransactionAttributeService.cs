using Device.FingerPrint.Core;
using Device.FingerPrint.Core.Encoders;
using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Features.LogInfo.Model;
using Device.FingerPrint.Features.LogInfo.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.Model.DTO;
using Device.FingerPrint.Model.Enums;
using Device.FingerPrint.Redis;
using DeviceDetectorNET;
using DeviceDetectorNET.Class.Device;
using Fanex.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static MassTransit.ValidationResultExtensions;

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
                    var sessionKey = Utilities.ParseSessionKey(model.SessionKey, appSettings.RedisDatabase);

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

        private InsertFPFingerprintDTO TransformFingerprintAttributes(RawTransactionModel rawTransModel, FingerprintModel fpModel, HeaderModel headerModel)
        {
            var result = new InsertFPFingerprintDTO();
            var hasFPFingerPrint = false;
            var hasFPFingerPrintExtra = false;

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

                // Fingerprint Extra
                var fingerprintAttributeExtra = new FPFingerprintAttributeExtraModel();
                if (headerModel != null)
                {
                    fingerprintAttributeExtra.Platform = fpModel.Platform;

                    fingerprintAttributeExtra.Accept = headerModel.Accept;
                    fingerprintAttributeExtra.Encoding = headerModel.Encoding;
                    fingerprintAttributeExtra.Headers = headerModel.Headers;

                    hasFPFingerPrintExtra = true;
                }

                var userAgentInfo = rawTransModel.UserAgentInfo;
                if (userAgentInfo != null)
                {
                    fingerprintAttributeExtra.UserAgent = userAgentInfo.UserAgent;
                    fingerprintAttributeExtra.BrowserName = userAgentInfo.ClientName;
                    fingerprintAttributeExtra.BrowserVersion = userAgentInfo.ClientVersion;
                    fingerprintAttributeExtra.OSName = userAgentInfo.OSName;

                    hasFPFingerPrintExtra = true;
                }

                // Fingerprint Old
                fingerprintAttributeExtra.Navigator = "eeaad5080693867762d6caec71976f3b";
                fingerprintAttributeExtra.Plugins = "fa3a2d33dd4f9121a5da467a2cc5e0e4";
                fingerprintAttributeExtra.Mimetypes = "929150a47300f5af954870efaa455134";
                fingerprintAttributeExtra.Useragent = "362d7fe3d8b2581bffa359f0eeda7106";
                fingerprintAttributeExtra.Fonts = "d4b61ce8d87405edba83ff57c7bacae6";
                fingerprintAttributeExtra.Screen = "ac34d0a02701999d6f6aa3c74abe6bfa";
                fingerprintAttributeExtra.Monitor = "1920@1080@24";
                fingerprintAttributeExtra.Timezone = -420;
                fingerprintAttributeExtra.GPUCode = "26e59377e714f98fea2c8fcab040a825";
                fingerprintAttributeExtra.WebGL = "6b126b1881442159a392c37841667b14";
                fingerprintAttributeExtra.Language = "en-US";
                fingerprintAttributeExtra.CookieEnabled = true;
                fingerprintAttributeExtra.DoNotTrack = false;
                fingerprintAttributeExtra.SuperCookies = "110010";
                fingerprintAttributeExtra.SystemTimeOffset = 0;
                fingerprintAttributeExtra.Browser = "Chrome 127.0.0.0";
                fingerprintAttributeExtra.DeviceType = GetDeviceType(rawTransModel);

                if (hasFPFingerPrint)
                {
                    result.FPFingerPrintCode = crypto.GetMD5Hash(JsonConvert.SerializeObject(fingerPrintAttribute));
                    result.FPFingerPrintAttribute = fingerPrintAttribute;

                    // Fingerprint Old
                    result.FPFingerPrintOldCode = GetOldFingerprintCode(fingerprintAttributeExtra);
                    
                }

                if (hasFPFingerPrintExtra)
                {
                    result.FPFingerPrintExtraCode = crypto.GetMD5Hash(JsonConvert.SerializeObject(fingerprintAttributeExtra));
                    result.FPFingerPrintExtraAttribute = fingerprintAttributeExtra;
                }
            }

            return result;
        }

        public static Dictionary<string, string> ConvertToStringDictionary(Dictionary<string, object> dictionary)
        {
            var result = new Dictionary<string, string>();

            foreach (var kvp in dictionary)
            {
                result[kvp.Key] = JsonConvert.SerializeObject(kvp.Value);
            }

            return result;
        }

        public static string ConvertToString(object value)
        {
            if (value is JObject jObject)
            {
                return jObject.ToString();
            }
            else if (value is JArray jArray)
            {
                return jArray.ToString();
            }
            else
            {
                return value?.ToString();
            }
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

            userAgentInfo.FPUserAgentCode = crypto.GetMD5Hash(userAgent);

            return userAgentInfo;
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

        private int GetDeviceType(RawTransactionModel model)
        {
            int _DEFAULTINDEX = 0;
            var deviceNames = new string[] { "others", "desktop", "tablet", "smartphone" };
            var index = Array.IndexOf(deviceNames, model.UserAgentInfo.Device.ToLower());
            return index > -1 ? index : _DEFAULTINDEX;
        }

        private string GetOldFingerprintCode(FPFingerprintAttributeExtraModel fingerprintAttributeExtra)
        {
            var codes = new string[]
            {
                fingerprintAttributeExtra.Navigator,
                fingerprintAttributeExtra.Plugins,
                fingerprintAttributeExtra.Mimetypes,
                fingerprintAttributeExtra.Useragent,
                fingerprintAttributeExtra.Fonts,
                fingerprintAttributeExtra.Screen,
                fingerprintAttributeExtra.Monitor,
                fingerprintAttributeExtra.Timezone.ToString(),
                fingerprintAttributeExtra.GPUCode,
                fingerprintAttributeExtra.WebGL,
                fingerprintAttributeExtra.Language,
                fingerprintAttributeExtra.CookieEnabled.ToString(),
                fingerprintAttributeExtra.DoNotTrack.ToString(),
                fingerprintAttributeExtra.SuperCookies,
                fingerprintAttributeExtra.SystemTimeOffset.ToString(),
                fingerprintAttributeExtra.Browser,
                fingerprintAttributeExtra.DeviceType.ToString()
            };
            codes = Array.FindAll(codes, x => !string.IsNullOrEmpty(x));
            return crypto.GetMD5Hash(string.Join("$", codes));
        }

        private bool GetIsIncognitoMode(FPFingerprintAttributeModel fingerprintAttribute)
        {
            bool isIncognitoMode = false;
            bool.TryParse(fingerprintAttribute.IsIncognitoMode, out isIncognitoMode);
            return isIncognitoMode;
        }
        #endregion    
    }

}
