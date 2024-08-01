using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
        Task InsertRawTransaction(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra);

        Task InsertFPTagging(string taggings, bool isTransOfDBExtra);

        Task InsertFPUserAgent(string userAgentJson, bool isTransOfDBExtra);

        Task InsertJSChallengeInfo(string jsChallengeInfoJson, bool isTransOfDBExtra);

        Task InsertFPFingerPrint(string fingerPrintJson, bool isTransOfDBExtra);

        void InsertTransaction(IEnumerable<TransactionEntity> transactions);

        void DeviceIdentify(string transJson, int groupType);

        void DeviceIdentifyNoFingerprint(string rawTransIds);

        (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity>, IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch);

        void MergeTransaction(string transJson);

        IEnumerable<MergeTransEntity> GetMergeTransaction(int batchSize, int retry);

        IEnumerable<MergeTransEntity> GetForceMergeTransaction(int batchSize, int retry);

        void ForceMergeTransaction(string transJson);

        IEnumerable<TransactionEvolutionEntity> GetTransactionEvolution(int batchSize);

        IEnumerable<TransactionEvolutionAttributesEntity> GetTransactionEvolutionAttributes(string fingerPrintIds, string BrowserFamilyCode);

        void EvolutionNewDevice(string transJson);

        void EvolutionRecoverDevice(string transJson);

        IEnumerable<TransactionHistoryEntity> GetTransactionHistoryByTransIdSubscriber(int subscriberId, long transId, int limit);

        (IEnumerable<AssociationDeviceEntity>, IEnumerable<AssociationAccountEntity>) GetAssociationByAccountSubscriber(int subscriberId, string account);

        IEnumerable<AssociationAccountUsageEntity> GetAssociationBySubscriberDevice(int subscriberId, string deviceCode, DateTime fromDate, DateTime toDate);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionDataAccess transactionDataAccess;

        public TransactionService(ITransactionDataAccess transactionDataAccess)
        {
            this.transactionDataAccess = transactionDataAccess;
        }

        public async Task InsertRawTransaction(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra)
        {
            var transJson = JsonConvert.SerializeObject(TransformInsertRawTransaction(transactions));
            await transactionDataAccess.InsertRawTransaction(transJson, isTransOfDBExtra);

        }

        public async Task InsertFPTagging(string taggings, bool isTransOfDBExtra)
        {
            await transactionDataAccess.InsertFPTagging(taggings, isTransOfDBExtra);
        }

        public async Task InsertFPUserAgent(string userAgentJson, bool isTransOfDBExtra)
        {
            await transactionDataAccess.InsertFPUserAgent(userAgentJson, isTransOfDBExtra);
        }

        public async Task InsertJSChallengeInfo(string jsChallengeInfoJson, bool isTransOfDBExtra)
        {
            await transactionDataAccess.InsertJSChallengeInfo(jsChallengeInfoJson, isTransOfDBExtra);
        }

        public async Task InsertFPFingerPrint(string fingerPrintJson, bool isTransOfDBExtra)
        {
            await transactionDataAccess.InsertFPFingerPrint(fingerPrintJson, isTransOfDBExtra);
        }

        private static IEnumerable<InsertRawTransactionDTO> TransformInsertRawTransaction(IEnumerable<RawTransactionModel> transactions)
        {
            return transactions.Select(transaction => new InsertRawTransactionDTO
            {
                Action = transaction.Action,
                ActionResult = transaction.ActionResult,
                BotComponent = transaction.BotComponent,
                BotDetection = transaction.BotDetection,
                ChallengeCode = transaction.ChallengeCode,
                FPTagging = transaction.FPTagging,
                CreatedDate = transaction.CreatedDate,
                DeviceCode = transaction.DeviceCode,
                FakeIP = transaction.FakeIP,
                FingerprintCode = transaction.FingerprintCode,
                FingerprintMoreInfo = transaction.FingerprintMoreInfo,
                FlaggedCode = transaction.FlaggedCode,
                FPSTransId = transaction.FPSTransId,
                InvalidDevice = transaction.InvalidDevice,
                IP = transaction.IP,
                IPId = transaction.IPId,
                IsIPV6 = transaction.IsIPV6,
                LoginName = transaction.LoginName,
                PluginID = transaction.PluginID,
                FPFingerPrintCode = transaction.FPFingerPrintCode,
                FPFingerPrintExtraCode = transaction.FPFingerPrintExtraCode,
                FPUserAgentCode = transaction.FPUserAgentCode,
                SubscriberName = transaction.SubscriberName,
                TransStatus = transaction.TransStatus,
                TransTime = transaction.TransTime,
                URL = transaction.URL,
                UserAgent = transaction.UserAgent
            }).ToList();
        }

        public void InsertTransaction(IEnumerable<TransactionEntity> transactions)
        {
            var transJson = JsonConvert.SerializeObject(transactions);
            transactionDataAccess.InsertTransaction(transJson);
        }

        public void DeviceIdentify(string transJson, int groupType)
        {
            transactionDataAccess.DeviceIdentify(transJson, groupType);
        }

        public void DeviceIdentifyNoFingerprint(string rawTransIds)
        {
            transactionDataAccess.DeviceIdentifyNoFingerprint(rawTransIds);
        }

        public void ForceMergeTransaction(string transJson)
        {
            transactionDataAccess.UpdateForceMergeTransaction(transJson);
        }

        public (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity>, IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch)
        {
            return transactionDataAccess.GetDeviceIdentify(batchSize, numberOfBatch);
        }

        public IEnumerable<TransactionEvolutionAttributesEntity> GetTransactionEvolutionAttributes(string fingerPrintIds, string BrowserFamilyCode)
        {
            return transactionDataAccess.GetTransactionEvolutionAttributes(fingerPrintIds, BrowserFamilyCode);
        }

        public IEnumerable<TransactionEvolutionEntity> GetTransactionEvolution(int batchSize)
        {
            return transactionDataAccess.GetTransactionEvolution(batchSize);
        }

        public IEnumerable<MergeTransEntity> GetForceMergeTransaction(int batchSize, int retry)
        {
            return transactionDataAccess.GetForceMergeTransaction(batchSize, retry);
        }

        public IEnumerable<MergeTransEntity> GetMergeTransaction(int batchSize, int retry)
        {
            return transactionDataAccess.GetMergeTransaction(batchSize, retry);
        }

        public void MergeTransaction(string transJson)
        {
            transactionDataAccess.UpdateMergeTransaction(transJson);
        }

        public void EvolutionNewDevice(string transJson)
        {
            transactionDataAccess.EvolutionNewDevice(transJson);
        }

        public void EvolutionRecoverDevice(string transJson)
        {
            transactionDataAccess.EvolutionRecoverDevice(transJson);
        }

        public IEnumerable<TransactionHistoryEntity> GetTransactionHistoryByTransIdSubscriber(int subscriberId, long transId, int limit)
        {
            return transactionDataAccess.GetTransactionHistoryByTransIdSubscriber(subscriberId, transId, limit);
        }

        public (IEnumerable<AssociationDeviceEntity>, IEnumerable<AssociationAccountEntity>) GetAssociationByAccountSubscriber(int subscriberId, string account)
        {
            return transactionDataAccess.GetAssociationByAccountSubscriber(subscriberId, account);
        }

        public IEnumerable<AssociationAccountUsageEntity> GetAssociationBySubscriberDevice(int subscriberId, string deviceCode, DateTime fromDate, DateTime toDate)
        {
            return transactionDataAccess.GetAssociationBySubscriberDevice(subscriberId, deviceCode, fromDate, toDate);
        }
    }
}
