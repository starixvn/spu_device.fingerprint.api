using Fanex.Data.Repository;
using Device.FingerPrint.Features.Transaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Device.FingerPrint.Model;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionDataAccess
    {
        void InsertTransaction(string jsonTransaction);

        void DeviceIdentify(string json, string offsetName);

        void DeviceIdentifyNoFingerprint(string json);

        IEnumerable<DeviceTransactionEntity> GetDeviceIdentify(int batchSize, string offsetName);

        void MergeTransaction(int batchSize);

        void MergeTransactionNew(string transJson);

        void UpdateTransaction(string rawTransJson, int retry, int offsetId, long offsetValue);

        void UpdateTransactionN(string rawTransJson, int offsetId, long offsetValue);
    }

    public class TransactionDataAccess : ITransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertTransaction(string jsonTransaction)
        {
            var criteria = new InsertTransactionCriteria
            {
                Transaction = jsonTransaction
            };

            repository.Execute(criteria);
        }

        public void DeviceIdentify(string transJson, string offsetName)
        {
            var criteria = new DeviceIdentifyCriteria
            {
                TransJson = transJson,
                OffsetName = offsetName
            };

            repository.Execute(criteria);
        }

        public void DeviceIdentifyNoFingerprint(string transJson)
        {
            var criteria = new DeviceIdentifyNoFingerprintCriteria
            {
                TransJson = transJson
            };

            repository.Execute(criteria);
        }

        public IEnumerable<DeviceTransactionEntity> GetDeviceIdentify(int batchSize, string offsetName)
        {
            var criteria = new DeviceIdentifyGetCriteria
            {
                Size = batchSize,
                OffsetName = offsetName
            };

            return repository.Fetch<DeviceTransactionEntity>(criteria);
        }

        public void MergeTransaction(int batchSize)
        {
            var criteria = new MergeTransactionCriteria
            {
               Size  = batchSize
            };

            repository.Execute(criteria);
        }

        public void MergeTransactionNew(string transJson)
        {
            var criteria = new MergeTransactionNewCriteria
            {
                TransJson = transJson
            };

            repository.Execute(criteria);
        }

        public void UpdateTransaction(string rawTransJson, int retry, int offsetId, long offsetValue)
        {
            var criteria = new UpdateTransactionCriteria
            {
                RawTransJson = rawTransJson,
                Retry = retry,
                OffsetId = offsetId,
                OffsetValue = offsetValue
            };

            repository.Execute(criteria);
        }

        public void UpdateTransactionN(string rawTransJson, int offsetId, long offsetValue)
        {
            var criteria = new UpdateTransactionNCriteria
            {
                RawTransJson = rawTransJson,
                OffsetId = offsetId,
                OffsetValue = offsetValue
            };

            repository.Execute(criteria);
        }
    }
}
