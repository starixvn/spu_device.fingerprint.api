using Device.FingerPrint.Model;
using Device.FingerPrint.PortalAPI.Models;
using Newtonsoft.Json;
using System;

namespace Device.FingerPrint.PortalAPI.Services
{
    public interface ITransactionService
    {
        TransactionModel MergeData(TransactionModel model);
    }

    public class TransactionService : ITransactionService
    {
        private readonly IRedisService redisService;

        public TransactionService(IRedisService redisService)
        {
            this.redisService = redisService;
        }

        public TransactionModel MergeData(TransactionModel model)
        {
            var cookie = redisService.GetCookie(model.SubscriberId, model.SessionKey);
            var fingerPrint = redisService.GetFingerPrint(model.SubscriberId, model.SessionKey);
            var header = redisService.GetHeader(model.SubscriberId, model.SessionKey);

            var headless = redisService.GetHeadless(model.SubscriberId, model.SessionKey);
            var browserInfo = redisService.GetBrowserInfo(model.SubscriberId, model.SessionKey);

            model.CookieCode = cookie;
            model.FingerprintAttribute = TransformFingerprintAttributes(fingerPrint, header);

            return model;
        }

        private FingerprintAttributeModel TransformFingerprintAttributes(FingerprintModel model, HeaderModel headerModel)
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
                result.BrowserName = headerModel.BrowserName;
                result.BrowserVersion = headerModel.BrowserVersion;
                result.OSName = headerModel.OSName;
            }

            return result;
            //return JsonConvert.SerializeObject(result);
        }
    }
}
