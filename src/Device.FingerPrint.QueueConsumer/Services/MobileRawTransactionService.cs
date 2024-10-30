using Device.FingerPrint.Model;
using System;
using System.Collections;
using Device.FingerPrint.Core;
using Fanex.Logging;

namespace Device.FingerPrint.QueueConsumer.Services
{
    public interface IMobileRawTransactionService
    {
        RawTransactionModel TransformRawTransaction(MessageQueue message);
    }

    public class MobileRawTransactionService : IMobileRawTransactionService
    {
        private readonly IUserAgentService userAgentService;
        private readonly IAppSettings appSettings;

        public MobileRawTransactionService(
            IUserAgentService userAgentService,
            IAppSettings appSettings)
        {
            this.userAgentService = userAgentService;
            this.appSettings = appSettings;
        }

        public RawTransactionModel TransformRawTransaction(MessageQueue message)
        {
            try
            {
                var rawTransModel = new RawTransactionModel
                {
                    LoginName = message.Account,
                    SubscriberName = message.Subscriber,
                    SubscriberId = message.SubscriberId,
                    TransTime = Convert.ToDateTime(message.TransDate.ToString("yyyy-MM-dd HH:mm:ss.ffff")),
                    Action = message.Action,
                    ActionResult = message.ActionResult,
                    MBAndroidCode = message.Mobile.AndroidCode,
                    MBCustomCode = message.Mobile.CustomCode,
                    MBMediaDRMCode = message.Mobile.MediaDRMCode,
                    MBIsMobileApp = message.Mobile.IsMobileAppTrans,
                    MBIsEmulator = message.Mobile.IsEmulator,
                    MBModelName = message.Mobile.ModelName,
                    MBManufacturer = message.Mobile.Manufacturer,
                    MBBrand = message.Mobile.Brand,
                    MBOS = message.Mobile.OS,
                    MBTimezone = message.Mobile.Timezone,
                    MBLanguage = message.Mobile.SystemLanguage,
                    MBCountry = message.Mobile.Country,
                    MBSDKVersion = message.Mobile.SdkVersion,
                };

                rawTransModel.TransStatus = GetTransStatus(message);

                rawTransModel.FlaggedCode = GetFlaggedCode();

                TransformIP(message, rawTransModel);

                return rawTransModel;
            }
            catch (Exception ex)
            {
                _ = LogService.ErrorAsync($"{appSettings.Environment} - Transform Mobile RawTransaction has error:\n {ex.Message}");
            }

            return new RawTransactionModel
            {
                TransStatus = (int)TransStatusEnum.InvalidLoginName
            };
        }

        private int GetTransStatus(MessageQueue message)
        {
            var transBit = new BitArray(16);

            if (Core.Utilities.ContainsUnicodeCharacter(message.Account))
            {
                transBit.Set((int)TransStatusEnum.InvalidLoginName, true);
            }

            if (string.IsNullOrEmpty(message.Mobile.AndroidCode)
                && string.IsNullOrEmpty(message.Mobile.CustomCode)
                && string.IsNullOrEmpty(message.Mobile.MediaDRMCode))
            {
                transBit.Set((int)TransStatusEnum.DeviceCodeIsEmpty, true);
            }

            return Core.Utilities.ConvertBitArray2Int(transBit);
        }

        private string GetFlaggedCode()
        {
            return Flagged.Human.ToString();
        }

        private void TransformIP(MessageQueue message, RawTransactionModel model)
        {
            var realIP = message.IP?.Trim();
            var ipConverted = realIP.Ip2Long();

            model.IP = realIP;
            model.IPId = ipConverted.IpNumber;
            model.IsIPV6 = ipConverted.IsIpV6;
        }

    }
}
