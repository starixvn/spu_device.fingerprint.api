using Device.FingerPrint.Features.Transaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionDataAccess
    {
        Task InsertRawTransaction(string transJson, bool isTransOfDBExtra);

        void InsertTransaction(string transJson);

        void DeviceIdentify(string transJson, int groupType);

        void DeviceIdentifyNoFingerprint(string rawTransIds);

        (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity>, IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch);

        void UpdateMergeTransaction(string transJson);

        IEnumerable<MergeTransEntity> GetMergeTransaction(int batchSize, int retry);

        IEnumerable<MergeTransEntity> GetForceMergeTransaction(int batchSize, int retry);

        void UpdateForceMergeTransaction(string transJson);

        IEnumerable<TransactionEvolutionEntity> GetTransactionEvolution(int batchSize);

        IEnumerable<TransactionEvolutionAttributesEntity> GetTransactionEvolutionAttributes(string fingerPrintIds, string BrowserFamilyCode);

        void EvolutionNewDevice(string transJson);

        void EvolutionRecoverDevice(string transJson);

        IEnumerable<LatestTransactionEntity> GetLatestTransaction(int subscriberId, int length, int skip, int take, out int totalItem);

        IEnumerable<TransactionHistoryEntity> GetTransactionHistory(int subscriberId, DateTime fromDate, DateTime toDate, string username, bool IsPartial, int skip, int take, out int totalItem);

    }

    public class TransactionDataAccess : ITransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public async Task InsertRawTransaction(string transJson, bool isTransOfDBExtra)
        {
            var criteria = new InsertRawTransactionCriteria
            {
                RawTransJson = transJson,
                IsTransOfDBExtra = isTransOfDBExtra
            };

            await repository.ExecuteAsync(criteria);
        }

        public void InsertTransaction(string transJson)
        {
            var criteria = new InsertTransactionCriteria
            {
                Transaction = transJson
            };

            repository.Execute(criteria);
        }

        public void DeviceIdentify(string transJson, int groupType)
        {
            var criteria = new DeviceIdentifyCriteria
            {
                TransJson = transJson,
                GroupType = groupType
            };

            repository.Execute(criteria);
        }

        public void DeviceIdentifyNoFingerprint(string rawTransIds)
        {
            var criteria = new DeviceIdentifyNoFingerprintCriteria
            {
                TransList = rawTransIds
            };

            repository.Execute(criteria);
        }

        public (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity>, IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch)
        {
            var criteria = new DeviceIdentifyGetCriteria
            {
                Size = batchSize,
                NumberOfBatch = numberOfBatch
            };

            return repository.FetchMultiple<DeviceIdentifyEntity, DeviceIdentifyEntity, ulong>(criteria);
        }

        public IEnumerable<TransactionEvolutionAttributesEntity> GetTransactionEvolutionAttributes(string fingerPrintIds, string BrowserFamilyCode)
        {
            var criteria = new GetTransactionAttributesEvolutionCriteria
            {
                FingerPrintIds = fingerPrintIds,
                BrowserFamilyCode = BrowserFamilyCode
            };

            return repository.Fetch<TransactionEvolutionAttributesEntity>(criteria);
        }

        public IEnumerable<TransactionEvolutionEntity> GetTransactionEvolution(int batchSize)
        {
            var criteria = new GetTransactionEvolutionCriteria
            {
                Size = batchSize
            };

            return repository.Fetch<TransactionEvolutionEntity>(criteria);
        }

        public IEnumerable<MergeTransEntity> GetForceMergeTransaction(int batchSize, int retry)
        {
            var criteria = new GetForceMergeTransactionCriteria
            {
                Size = batchSize,
                Retry = retry
            };

            return repository.Fetch<MergeTransEntity>(criteria);
        }

        public IEnumerable<MergeTransEntity> GetMergeTransaction(int batchSize, int retry)
        {
            var criteria = new GetMergeTransactionCriteria
            {
                Size = batchSize,
                Retry = retry
            };

            return repository.Fetch<MergeTransEntity>(criteria);
        }

        public void UpdateForceMergeTransaction(string transJson)
        {
            var criteria = new UpdateForceMergeTransactionCriteria
            {
                Transaction = transJson
            };

            repository.Execute(criteria);
        }

        public void UpdateMergeTransaction(string transJson)
        {
            var criteria = new UpdateMergeTransactionCriteria
            {
                Transaction = transJson
            };

            repository.Execute(criteria);
        }

        public void EvolutionNewDevice(string transJson)
        {
            var criteria = new EvolutionNewDeviceCriteria
            {
                Transaction = transJson
            };

            repository.Execute(criteria);
        }

        public void EvolutionRecoverDevice(string transJson)
        {
            var criteria = new EvolutionRecoverDeviceCriteria
            {
                Transaction = transJson
            };

            repository.Execute(criteria);
        }

        public IEnumerable<LatestTransactionEntity> GetLatestTransaction(int subscriberId, int length, int skip, int take, out int totalItem)
        {
            var criteria = new GetLatestTransactionBySubscriberCriteria
            {
               SubscriberId =   subscriberId,
               Length   = length,
               Skip = skip,
               Take = take,
               TotalItem = 0
            };

            totalItem = criteria.TotalItem;

            return repository.Fetch<LatestTransactionEntity>(criteria);
        }

        public IEnumerable<TransactionHistoryEntity> GetTransactionHistory(int subscriberId, DateTime fromDate, DateTime toDate, string username, bool IsPartial, int skip, int take, out int totalItem)
        {
            var criteria = new GetTransactionHistoryCriteria
            {
                FromDate = fromDate,
                ToDate = toDate,
                IsPartial = IsPartial,
                SubscriberId = subscriberId,
                UserName = username,
                Skip = skip,
                Take = take,
                TotalItem = 0
            };

            totalItem = criteria.TotalItem;

            return repository.Fetch<TransactionHistoryEntity>(criteria);
        }
    }
}
