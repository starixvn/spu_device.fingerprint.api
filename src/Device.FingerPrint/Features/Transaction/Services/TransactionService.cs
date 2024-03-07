using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
        Task InsertRawTransaction(IEnumerable<RawTransactionModel> transactions, bool isTransOfDBExtra);

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

        IEnumerable<LatestTransactionEntity> GetLatestTransaction(int subscriberId, int length, int skip, int take, out int totalItem);

        IEnumerable<TransactionHistoryEntity> GetTransactionHistory(int subscriberId, DateTime fromDate, DateTime toDate, string username, bool IsPartial, int skip, int take, out int totalItem);
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
            var transJson = JsonConvert.SerializeObject(transactions);
            await transactionDataAccess.InsertRawTransaction(transJson, isTransOfDBExtra);
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

        public IEnumerable<LatestTransactionEntity> GetLatestTransaction(int subscriberId, int length, int skip, int take, out int totalItem)
        {
            return transactionDataAccess.GetLatestTransaction(subscriberId, length, skip, take, out totalItem);
        }

        public IEnumerable<TransactionHistoryEntity> GetTransactionHistory(int subscriberId, DateTime fromDate, DateTime toDate, string username, bool IsPartial, int skip, int take, out int totalItem)
        {
            return transactionDataAccess.GetTransactionHistory(subscriberId, fromDate, toDate, username, IsPartial, skip, take, out totalItem);
        }
    }
}
