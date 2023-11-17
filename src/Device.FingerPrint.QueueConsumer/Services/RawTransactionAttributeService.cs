using Device.FingerPrint.Core.Services;
using Device.FingerPrint.Model;
using Device.FingerPrint.Redis;
using DeviceDetectorNET;
using Newtonsoft.Json;
using System.Threading.Tasks;

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

        public RawTransactionAttributeService(ICrypto crypto, IRedisService redisService) { 
            this.crypto = crypto;
            this.redisService = redisService;
        }

        public async Task GetTransactionAttribute(RawTransactionModel model)
        {
            if (model != null)
            {
                var fingerPrintTask = redisService.HashGetAsync<FingerprintModel>(model.SessionKey, RedisString.GetRedisKeyName(RedisKey.FingerPrint));
                var headerTask = redisService.HashGetAsync<HeaderModel>(model.SessionKey, RedisString.GetRedisKeyName(RedisKey.Header));
                var cookieTask = redisService.HashGetAsync<string>(model.SessionKey, RedisString.GetRedisKeyName(RedisKey.Cookie));

                await Task.WhenAll(fingerPrintTask, headerTask, cookieTask);

                var fingerPrint = fingerPrintTask.Result;
                var header = headerTask.Result;
                var cookie = cookieTask.Result;

                model.RawUserAgent = model.UserAgent;
                model.RawUserAgentInfo = GetUserAgentInfo(model.UserAgent);

                model.FingerprintAttribute = TransformFingerprintAttributes(fingerPrint, header, model.RawUserAgentInfo);
                model.CookieCode = cookie;
            }
        }

        private FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel fpModel, HeaderModel headerModel, UserAgentInfoModel userAgentInfo)
        {
            if (fpModel == null) return new FingerprintAttributeModel();

            var result = new FingerprintAttributeModel
            {
                FingerPrintCode = fpModel.Id,
                Attribute = fpModel.Attributes,
                Platform = fpModel.Platform
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
    }
}
