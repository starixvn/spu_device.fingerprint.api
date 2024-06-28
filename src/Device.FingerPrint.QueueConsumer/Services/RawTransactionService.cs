namespace Device.FingerPrint.QueueConsumer.Services
{
    using Device.FingerPrint.Core;
    using Device.FingerPrint.Features.Transaction.Model;
    using Device.FingerPrint.Features.Transaction.Services;
    using Device.FingerPrint.Model;
    using Device.FingerPrint.Redis;
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using static MassTransit.ValidationResultExtensions;

    public interface IRawTransactionService
    {
        Task<RawTransactionModel> TransformRawTransaction(MessageQueue message);

        Task ClearRedisAsync(IEnumerable<RawTransactionModel> transactions);

        Task ClearRedisJsChallenge(IEnumerable<string> jsChallengeCodes);

        Task InsertOrUpdateEverCookie(string hashKey, IEnumerable<string> everCookies, long expireSecond);

        Task InsertRawTransactionDetail(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra);
    }

    public class RawTransactionService : IRawTransactionService
    {
        private readonly IRawTransactionAttributeService rawTransactionAttributeService;
        private readonly IRedisService redisService;
        private readonly IRedisMultiplexService redisMultiplexService;
        private readonly IDeviceService deviceService;
        private readonly IBotDetectionService botDetectionService;
        private readonly ITransactionService transactionService;

        public RawTransactionService(
            IRedisService redisService,
            IRedisMultiplexService redisMultiplexService,
            IRawTransactionAttributeService rawTransactionAttributeService,
            IDeviceService deviceService,
            IBotDetectionService botDetectionService,
            ITransactionService transactionService
        )
        {
            this.rawTransactionAttributeService = rawTransactionAttributeService;
            this.redisService = redisService;
            this.redisMultiplexService = redisMultiplexService;
            this.deviceService = deviceService;
            this.botDetectionService = botDetectionService;
            this.transactionService = transactionService;
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
            var botDetection = botDModel?.IsVer3 == true ? botDModel?.Detection : botDetectionService.GetBotDetectionTrue(botDModel);

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
                BotDetection = botDetection,
                BotComponent = botDModel?.Component ?? string.Empty
            };

            await rawTransactionAttributeService.GetTransactionAttribute(rawTransModel, botDModel);

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

        public async Task ClearRedisJsChallenge(IEnumerable<string> jsChallengeCodes)
        {
            var tasks = jsChallengeCodes
            .Select(code => redisService.RedisJSChallengeDatabase.RemoveAsync($"Challenge:{code}", StackExchange.Redis.CommandFlags.FireAndForget))
            .ToList();

            await Task.WhenAll(tasks);
        }

        public async Task InsertOrUpdateEverCookie(string hashKey, IEnumerable<string> everCookies, long expireSecond)
        {
            try
            {
                var redisEverCookieDb = redisMultiplexService.GetRedisEverCookieDb();

                var tasks = everCookies
                .Select(code => redisMultiplexService.EverCookie_AddItemWithExpiryAsync(hashKey, code, DateTime.Now.ToString(), TimeSpan.FromSeconds(expireSecond)))
                .ToList();

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                var arr = ex.Message.Split(' ');
            }
        }

        public async Task InsertRawTransactionDetail(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            var tasks = new List<Task>
            {
                InsertCookie(transactions, isTransOfDBExtra),
                InsertRawUserAgent(transactions, isTransOfDBExtra),
                InsertJSChallengeInfo(transactions, isTransOfDBExtra),
                InsertRawFingerPrint(transactions, isTransOfDBExtra),
            };

            await Task.WhenAll(tasks);
        }

        private async Task InsertCookie(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            var cookieInfos = GetCookieInfo(transactions);
            if (cookieInfos != null && cookieInfos.Any())
            {
                var cookieJson = JsonConvert.SerializeObject(cookieInfos);
                await transactionService.InsertCookie(cookieJson, isTransOfDBExtra);
            }
        }

        private async Task InsertRawUserAgent(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            var rawUserAgentInfos = GetValidInfo(transactions, t => t.RawUserAgentInfo);
            if (rawUserAgentInfos != null && rawUserAgentInfos.Any())
            {
                var rawUserAgentJson = JsonConvert.SerializeObject(rawUserAgentInfos);
                await transactionService.InsertRawUserAgent(rawUserAgentJson, isTransOfDBExtra);
            }
        }

        private async Task InsertJSChallengeInfo(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            var jsChallengeInfos = GetValidInfo(transactions, t => t.RawJsChallengeInfo);
            if (jsChallengeInfos != null && jsChallengeInfos.Any())
            {
                var jsChallengeInfoJson = JsonConvert.SerializeObject(jsChallengeInfos);
                await transactionService.InsertJSChallengeInfo(jsChallengeInfoJson, isTransOfDBExtra);
            }
        }

        private async Task InsertRawFingerPrint(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            var rawFingerprintAttribute = GetValidInfo(transactions, t => t.FingerprintAttribute);
            if (rawFingerprintAttribute != null && rawFingerprintAttribute.Any())
            {
                var rawFingerPrintAttributeJson = JsonConvert.SerializeObject(rawFingerprintAttribute);
                await transactionService.InsertRawFingerPrint(rawFingerPrintAttributeJson, isTransOfDBExtra);
            }
        }

        private static IEnumerable<InsertCookieInfoDTO> GetCookieInfo(IEnumerable<RawTransactionModel> transactions)
        {
            var result = transactions
                .Where(t => !string.IsNullOrEmpty(t.CookieCode))
                .GroupBy(t => t.CookieCode)
                .Select(g => new InsertCookieInfoDTO
                {
                    CookieCode = g.Key,
                    FirstTransTime = g.Min(t => t.TransTime)
                })
                .ToList();

            return result;
        }

        private static IEnumerable<TResult> GetValidInfo<TResult>(IEnumerable<RawTransactionModel> transactions, Func<RawTransactionModel, TResult> selector)
        {
            return transactions
                .Select(selector)
                .Where(result => result != null)
                .Distinct();
        }

    }
}
