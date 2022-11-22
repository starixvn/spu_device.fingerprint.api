using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Ultilities;
using Newtonsoft.Json;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface ITransactionService
    {
        TransactionModel MergeData(TransactionModel model, string sessionKey);
    }

    public class TransactionService : ITransactionService
    {
        private readonly IRedisService redisService;
        private readonly IBrowserInfoService browserInfoService;
        private readonly Crypto crypto;

        public TransactionService(IAppSettings appSettings, IRedisService redisService, IBrowserInfoService browserInfoService)
        {
            this.redisService = redisService;
            this.browserInfoService = browserInfoService;
            crypto = new Crypto(appSettings.CryptoPassword, appSettings.CryptoIV, appSettings.CryptoSalt);
        }

        public TransactionModel MergeData(TransactionModel model, string sessionKey)
        {
            var cookie = redisService.GetCookie(model.SubscriberId, sessionKey);
            var fingerPrint = redisService.GetFingerPrint(model.SubscriberId, sessionKey);
            var header = redisService.GetHeader(model.SubscriberId, sessionKey);

            var headless = redisService.GetHeadless(model.SubscriberId, sessionKey);
            var browserInfo = redisService.GetBrowserInfo(model.SubscriberId, sessionKey);

            var (browserInfoModel, browserInfoAttributeModel) = browserInfoService.GetBrowserInfo(header, browserInfo);
            browserInfoModel.HeadlessScore = headless.Score;
            browserInfoModel.HeadlessDetail = headless.TestsDetail;

            var userAgentInfo = ParseUserAgentInfo(header.UserAgent);
            model.CookieCode = cookie;
            model.UserAgentInfo = userAgentInfo;
            model.FingerprintAttribute = TransformFingerprintAttributes(fingerPrint, header, browserInfo, userAgentInfo);
            model.BrowserInfo = browserInfoModel;
            model.BrowserInfoAttribute = browserInfoAttributeModel;

            return model;
        }

        private FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel model, HeaderModel headerModel, BrowserDetectModel browserInfo, UserAgentInfoModel userAgentInfo)
        {
            if (model == null) return new FingerprintAttributeModel();

            var result = new FingerprintAttributeModel
            {
                FingerPrintCode = model.Id,
                Attribute = model.Attributes,
            };

            if (headerModel != null)
            {
                result.Accept = headerModel.Accept;
                result.Encoding = headerModel.Encoding;
                result.Headers = headerModel.Headers;
                result.UserAgent = headerModel.UserAgent;

                result.BrowserName = userAgentInfo.ClientName;
                result.BrowserVersion = userAgentInfo.ClientVersion;
                result.OSName = userAgentInfo.OSName;
            }

            if (browserInfo != null)
            {
                result.Platform = browserInfo.Platform;
            }

            return result;
        }

        private UserAgentInfoModel ParseUserAgentInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return new UserAgentInfoModel();

            var transactionAttributeService = new TransactionAttributeService();
            return transactionAttributeService.GetUserAgentInfo(userAgent);
        }
    }
}
