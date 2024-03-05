using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Features.LogInfo.Model;
using Device.FingerPrint.Features.LogInfo.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.Model.Enums;
using Device.FingerPrint.Redis;
using DeviceDetectorNET;
using Fanex.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Device.FingerPrint.Core;
using System;
using Device.FingerPrint.Core.Encoders;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface IRawTransactionAttributeService
    {
        Task GetTransactionAttribute(RawTransactionModel model);
    }

    public class RawTransactionAttributeService : IRawTransactionAttributeService
    {
        private readonly ICrypto crypto;
        private readonly IRedisService redisService;
        private readonly ILogInfoServices logInfoServices;
        private readonly IAppSettings appSettings;
        private const string base64Key = "wFKfVDGZqRbyYCBi6IveMaQAh1ntWcP8ujT/U7r2xXs+J0L=4d3zEHSo5glN9kOpm";

        public RawTransactionAttributeService(ICrypto crypto, IRedisService redisService, ILogInfoServices logInfoServices, IAppSettings appSettings)
        {
            this.crypto = crypto;
            this.redisService = redisService;
            this.logInfoServices = logInfoServices;
            this.appSettings = appSettings;
        }

        public async Task GetTransactionAttribute(RawTransactionModel model)
        {
            if (model != null)
            {
                var fingerPrintTask = redisService.HashGetAsync<FingerprintModel>(model.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint));
                var fingerPrintEncodeTask = redisService.HashGetAsync<string>(model.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrintEncode));
                var headerTask = redisService.HashGetAsync<HeaderModel>(model.SessionKey, RedisString.GetRedisKeyName(RedisKey.Header));
                var cookieTask = redisService.HashGetAsync<string>(model.SessionKey, RedisString.GetRedisKeyName(RedisKey.Cookie));

                await Task.WhenAll(fingerPrintTask, fingerPrintEncodeTask, headerTask, cookieTask);

                var fingerPrint = fingerPrintTask.Result;
                var fingerPrintEncode = fingerPrintEncodeTask.Result;
                var header = headerTask.Result;
                var cookie = cookieTask.Result;

                model.RawUserAgent = model.UserAgent;
                model.RawUserAgentInfo = GetUserAgentInfo(model.UserAgent);

                var fingerPrintModel = new FingerprintModel();
                if (fingerPrint != null && !string.IsNullOrEmpty(fingerPrint.Id))
                {
                    fingerPrintModel = fingerPrint;
                }
                else if (!string.IsNullOrEmpty(fingerPrintEncode))
                {
                    var decodeData = Base64.Decode(fingerPrintEncode, base64Key);
                    fingerPrintModel = JsonConvert.DeserializeObject<FingerprintModel>(decodeData);
                }

                model.FingerprintAttribute = TransformFingerprintAttributes(fingerPrintModel, header, model.RawUserAgentInfo);
                model.CookieCode = cookie;

                // Log Info Consumer
                var subscriberName = model.SubscriberName;
                if (appSettings.GetValue<bool>("Consumer:LogInfo") 
                    && !subscriberName.Contains("Alpha", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(model.SessionKey))
                    {
                        LogSessionKeyEmpty(model);
                    }
                    else if (fingerPrint == null && cookie == null && header == null)
                    {
                        LogRedisValueEmpty(model);
                    }
                }

                var logUserTest = new[] { "member19", "qatest05", "andy7", "12mbtest005" };
                if (logUserTest.Contains(model.LoginName))
                {
                    _ = LogService.InfoAsync($"{appSettings.Environment} - user: {model.LoginName}\n" +
                                            $"RealIp: {header.RealIP}\n" +
                                            $"FakeIp: {header.FakeIP}");
                }
            }
        }

        private FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel fpModel, HeaderModel headerModel, UserAgentInfoModel userAgentInfo)
        {
            var result = new FingerprintAttributeModel();

            if (fpModel != null && !string.IsNullOrEmpty(fpModel.Id))
            {
                result.FingerPrintCode = fpModel.Id;
                result.Attribute = fpModel.Attributes;
                result.Platform = fpModel.Platform;
            }

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

        private UserAgentInfoModel GetUserAgentInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return new UserAgentInfoModel();

            var deviceDetector = new DeviceDetector(userAgent);

            deviceDetector.SkipBotDetection();
            deviceDetector.Parse();

            var userAgentInfo = new UserAgentInfoModel { UserAgent = userAgent };

            if (deviceDetector.IsBot())
            {
                var botInfo = deviceDetector.GetBot();
                if (botInfo != null && botInfo.Success)
                {
                    userAgentInfo.BotName = botInfo.Match.Name;
                    userAgentInfo.BotCategory = botInfo.Match.Category;
                }
                else
                {
                    userAgentInfo.BotName = "Not detected";
                }
            }

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
        #endregion
    }
}
