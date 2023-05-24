using System.Threading.Tasks;
using Device.FingerPrint.Core;
using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Device.FingerPrint.PortalAPI.Ultilities;
using Newtonsoft.Json;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface ITransactionService
    {

        Task<UserAgentInfoModel> ParseUserAgentInfo(string userAgent);

        FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel fpModel, HeaderModel headerModel, BrowserDetectModel browserInfo, UserAgentInfoModel userAgentInfo);

    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionAttributeService transactionAttributeService;
        private readonly ICrypto crypto;

        public TransactionService(ICrypto crypto, ITransactionAttributeService transactionAttributeService)
        {
            this.transactionAttributeService = transactionAttributeService;
            this.crypto = crypto;
        }        

        public FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel fpModel, HeaderModel headerModel, BrowserDetectModel browserInfo, UserAgentInfoModel userAgentInfo)
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

            result.BrowserInstanceCode = HashBrowserInstanceCode(result);
            result.BrowserFamilyCode = HashBrowserFamilyCode(result);

            return result;
        }

        public Task<UserAgentInfoModel> ParseUserAgentInfo(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent)) return null;

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
    }
}
