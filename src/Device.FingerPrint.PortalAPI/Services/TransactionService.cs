using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Ultilities;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface ITransactionService
    {
        TransactionModel MergeData(TransactionModel model, BrowserDetectModel browserInfo, string cookie, FingerprintModel fingerPrint, HeaderModel header, HeadlessBrowserModel headless);
    }

    public class TransactionService : ITransactionService
    {
        private readonly IBrowserInfoService browserInfoService;
        private readonly ITransactionAttributeService transactionAttributeService;
        private readonly Crypto crypto;

        public TransactionService(IAppSettings appSettings, IBrowserInfoService browserInfoService, ITransactionAttributeService transactionAttributeService)
        {
            this.browserInfoService = browserInfoService;
            this.transactionAttributeService = transactionAttributeService;
            crypto = new Crypto(appSettings.CryptoPassword, appSettings.CryptoIV, appSettings.CryptoSalt);
        }

        public TransactionModel MergeData(TransactionModel model, BrowserDetectModel browserInfo, string cookie, FingerprintModel fingerPrint, HeaderModel header, HeadlessBrowserModel headless)
        {
            var (browserInfoModel, browserInfoAttributeModel) = browserInfoService.GetBrowserInfo(header, browserInfo);

            if (headless != null)
            {
                browserInfoModel.HeadlessScore = headless.Score;
                browserInfoModel.HeadlessDetail = headless.TestsDetail;
                browserInfoModel.Flagged = browserInfoService.GetFlagged(browserInfoModel);
            }
            browserInfoModel.HeadlessDetail = headless.TestsDetail;

            var userAgentInfo = ParseUserAgentInfo(header);
            model.CookieCode = cookie;
            model.UserAgentInfo = userAgentInfo;

            browserInfoAttributeModel.BrowserInfoAttributeCode = crypto.GetMD5Hash(browserInfoAttributeModel.BrowserInfoAttributeJson);
            model.BrowserInfoAttribute = browserInfoAttributeModel;

            model.BrowserInfo = browserInfoModel;
            model.FingerprintAttribute = TransformFingerprintAttributes(fingerPrint, header, browserInfo, userAgentInfo);

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

            if (browserInfo != null)
            {
                result.Platform = browserInfo.Platform;
            }

            result.BrowserInstanceCode = crypto.GetMD5Hash(result.BrowserInstanceJson);
            result.BrowserFamilyCode = crypto.GetMD5Hash(result.BrowserFamilyJson);

            return result;
        }

        private UserAgentInfoModel ParseUserAgentInfo(HeaderModel headerModel)
        {
            if (headerModel == null) return new UserAgentInfoModel();

            return transactionAttributeService.GetUserAgentInfo(headerModel.UserAgent);
        }
    }
}
