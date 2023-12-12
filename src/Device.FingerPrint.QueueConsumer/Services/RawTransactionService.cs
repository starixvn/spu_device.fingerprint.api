namespace Device.FingerPrint.QueueConsumer.Services
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.QueueConsumer.Models;
    using Device.FingerPrint.Redis;
    using DeviceDetectorNET.Class;
    using Fanex.Logging;
    using Fanex.Oneyed.Infrastructure.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IRawTransactionService
    {
        Task<RawTransactionModel> TransformRawTransaction(MessageQueue message);

        Task ClearRedisAsync(IEnumerable<RawTransactionModel> transactions);
    }

    public class RawTransactionService : IRawTransactionService
    {
        private readonly IRawTransactionAttributeService rawTransactionAttributeService;
        private readonly IRedisService redisService;
        private readonly IDeviceService deviceService;
        private readonly IBotDetectionService botDetectionService;

        public RawTransactionService(
            IRedisService redisService,
            IRawTransactionAttributeService rawTransactionAttributeService,
            IDeviceService deviceService,
            IBotDetectionService botDetectionService
        )
        {
            this.rawTransactionAttributeService = rawTransactionAttributeService;
            this.redisService = redisService;
            this.deviceService = deviceService;
            this.botDetectionService = botDetectionService;
        }

        public async Task<RawTransactionModel> TransformRawTransaction(MessageQueue message)
        {
            var invalidDevice = string.Empty;
            var deviceCode = Utilities.GetDeviceCodeFromCapturedData(message.Device);
            var deviceInfo = deviceService.GetDeviceInfo(deviceCode, message);
            var url = Utilities.GetURLFromDeviceCode(message.Device);
            var userAgent = Utilities.GetUserAgentFromDeviceCode(message.Device);

            var transBit = new BitArray(16);
            if (string.IsNullOrWhiteSpace(deviceInfo.DeviceNo) || deviceInfo.DeviceNo == "N/A")
            {
                deviceInfo.DeviceNo = null;
                transBit.Set((int)TransStatusEnum.DeviceCodeIsEmpty, true);
            }
            else
            {
                if (!Utilities.IsGuidDeviceId(deviceInfo.DeviceNo) || deviceInfo.InvalidDeviceId)
                {
                    invalidDevice = deviceCode;
                    deviceInfo.DeviceNo = null;
                    transBit.Set((int)TransStatusEnum.InvalidDI, true);
                }
            }

            if (Utilities.ContainsUnicodeCharacter(message.Account))
            {
                transBit.Set((int)TransStatusEnum.InvalidLoginName, true);
            }

            var realIP = message.IP?.Trim();
            var ipConverted = realIP.Ip2Long();

            var botDModel = botDetectionService.TransformBotdModel(message);

            var flaggedCode = botDetectionService.GetFlaggedCode(botDModel, deviceInfo.DeviceNo, userAgent);

            var rawTransModel = new RawTransactionModel
            {
                DeviceCode = deviceInfo.DeviceNo,
                InvalidDevice = string.IsNullOrWhiteSpace(invalidDevice) ? null : invalidDevice,
                TransStatus = Core.Utilities.ConvertBitArray2Int(transBit),
                LoginName = message.Account,
                SubscriberName = message.Subscriber,
                SubscriberId = message.SubscriberId,
                TransTime = Convert.ToDateTime(message.TransDate.ToString("yyyy-MM-dd HH:mm:ss.ffff")),
                IP = realIP,
                IPId = ipConverted.IpNumber,
                FakeIP = message.FakeIP?.Trim(),
                IsIPV6 = ipConverted.IsIpV6,
                PluginID = 0,
                URL = string.IsNullOrWhiteSpace(url) ? Utilities.ConvertNA2Empty(Utilities.SubStringUrl(deviceInfo.HRef)) : Utilities.SubStringUrl(url),
                Action = message.Action,
                ActionResult = message.ActionResult,
                UserAgent = string.IsNullOrWhiteSpace(userAgent) ? Utilities.ConvertNA2Empty(Utilities.StripUnicodeCharactersFromString(deviceInfo.UserAgent)) : Utilities.StripUnicodeCharactersFromString(userAgent),
                FingerprintCode = deviceInfo.DeviceFPCode != "N/A" ? deviceInfo.DeviceFPCode : null,
                FingerprintMoreInfo = deviceInfo.DeviceFPCode != "N/A" ? Utilities.EncodeCapturedData(deviceInfo) : null,
                SessionKey = message.SessionKey,
                FlaggedCode = flaggedCode,
                BotDetection = botDModel?.Detection,
                BotComponent = botDetectionService.GetBotComponent(botDModel, flaggedCode)
            };

            await rawTransactionAttributeService.GetTransactionAttribute(rawTransModel);

            return rawTransModel;
        }

        public async Task ClearRedisAsync(IEnumerable<RawTransactionModel> transactions)
        {
            var groupByTrans = transactions
                .Where(x => x.SessionKey != null)
                .GroupBy(x => x.SessionKey);

            var tasks = groupByTrans
                .Select(transGroup => redisService.HashDeleteAsync(transGroup.Key, RedisString.GetRedisKeyNames()))
                .ToList();

            await Task.WhenAll(tasks);
        }

    }
}
