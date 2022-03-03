using Device.FingerPrint.Features.Transaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionDataAccess
    {
        void DeviceIdentify(string transJson, int groupType);

        void DeviceIdentifyNoFingerprint(string rawTransIds);

        (IEnumerable<DeviceIdentifyEntity>, IEnumerable<DeviceIdentifyEntity>, IEnumerable<ulong>) GetDeviceIdentify(int batchSize, int numberOfBatch);

        void UpdateMergeTransaction(string transJson);

        IEnumerable<MergeTransEntity> GetMergeTransaction(int batchSize, int retry);

        void UpdateForceMergeTransaction(string transJson);
    }

    public class TransactionDataAccess : ITransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionDataAccess()
        {
            repository = new DynamicRepository();
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
    }
}
