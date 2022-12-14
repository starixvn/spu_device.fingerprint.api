using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Ultilities;
using Newtonsoft.Json;

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
            }
            browserInfoModel.Flagged = browserInfoService.GetFlagged(browserInfoModel);

            var userAgentInfo = ParseUserAgentInfo(model.UserAgent);
            model.CookieCode = cookie;
            model.UserAgentInfo = userAgentInfo;

            browserInfoAttributeModel.BrowserInfoAttributeCode = HashBrowserInfoAttributeCode(browserInfoAttributeModel);
            model.BrowserInfoAttribute = browserInfoAttributeModel;

            browserInfoModel.BrowserInfoCode = HashBrowserInfoCode(browserInfoModel);
            model.BrowserInfo = browserInfoModel;

            model.FingerprintAttribute = TransformFingerprintAttributes(fingerPrint, header, browserInfo, userAgentInfo);

            return model;
        }

        private FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel fpModel, HeaderModel headerModel, BrowserDetectModel browserInfo, UserAgentInfoModel userAgentInfo)
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

            if (browserInfo != null)
            {
                result.Platform = browserInfo.Platform;
            }

            if (browserInfo != null)
            {
                result.Platform = browserInfo.Platform;
            }

            result.BrowserInstanceCode = HashBrowserInstanceCode(result);
            result.BrowserFamilyCode = HashBrowserFamilyCode(result);

            return result;
        }

        private UserAgentInfoModel ParseUserAgentInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return new UserAgentInfoModel();

            return transactionAttributeService.GetUserAgentInfo(userAgent);
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

        private string HashBrowserInfoCode(BrowserInfoModel model)
        {
            if (model == null) return string.Empty;

            var json = JsonConvert.SerializeObject(new BrowserInfoProp
            {
                OSName = model.OSName,
                BrowserName = model.BrowserName,
                HeadlessScore = model.HeadlessScore,
                HeadlessDetail = model.HeadlessDetail,
                Flagged = model.Flagged
            });

            return crypto.GetMD5Hash(json);
        }

        private string HashBrowserInfoAttributeCode(BrowserInfoAttributeModel model)
        {
            if (model == null) return string.Empty;

            var json = JsonConvert.SerializeObject(new BrowserInfoAttributeProp
            {
                SecChUa = model.SecChUa,
                SecChUaPlatform = model.SecChUaPlatform,
                SecChUaMobile = model.SecChUaMobile,
                JsBrowserName = model.JsBrowserName,
                JsOSName = model.JsOSName
            });

            return crypto.GetMD5Hash(json);
        }
    }
}
