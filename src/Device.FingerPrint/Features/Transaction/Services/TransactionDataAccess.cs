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
        void InsertJsonTransaction(string jsonTransaction);

        void DeviceIdentify(string json, string offsetName);

        void DeviceIdentifyNoFingerprint(string json);

        IEnumerable<DeviceTransactionEntity> GetDeviceIdentify(int batchSize, string offsetName);

        void MergeTransaction(int batchSize);

        void MergeTransactionNew(string transJson);

        void UpdateTransaction(string rawTransJson, int retry, string offsetName, long offsetvalue);

        void UpdateTransactionN(string rawTransJson, string offsetName, long offsetvalue);
    }

    public class TransactionDataAccess : ITransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertJsonTransaction(string jsonTransaction)
        {
            var criteria = new InsertJsonTransactionCriteria
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

        public void UpdateTransaction(string rawTransJson, int retry, string offsetName, long offsetvalue)
        {
            var criteria = new UpdateTransactionCriteria
            {
                RawTransJson = rawTransJson,
                Retry = retry,
                OffsetName = offsetName,
                Offsetvalue = offsetvalue
            };

            repository.Execute(criteria);
        }

        public void UpdateTransactionN(string rawTransJson, string offsetName, long offsetvalue)
        {
            var criteria = new UpdateTransactionNCriteria
            {
                RawTransJson = rawTransJson,
                OffsetName = offsetName,
                Offsetvalue = offsetvalue
            };

            repository.Execute(criteria);
        }
    }
}
