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

        void UpdateTransaction(TransactionUpdateConfigModel model);

        void UpdateTransactionN(TransactionUpdateConfigModel model);
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

        public void DeviceIdentify(string json, string offsetName)
        {
            var criteria = new DeviceIdentifyCriteria
            {
                Json = json,
                OffsetName = offsetName
            };

            repository.Execute(criteria);
        }

        public void DeviceIdentifyNoFingerprint(string json)
        {
            var criteria = new DeviceIdentifyNoFingerprintCriteria
            {
                Json = json
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

        public void UpdateTransaction(TransactionUpdateConfigModel model)
        {
            var criteria = new UpdateTransactionCriteria
            {
                Json = model.Json,
                Retry = model.Retry,
                OffsetName = model.OffsetName,
                Offsetvalue = model.Offsetvalue
            };

            repository.Execute(criteria);
        }

        public void UpdateTransactionN(TransactionUpdateConfigModel model)
        {
            var criteria = new UpdateTransactionNCriteria
            {
                Json = model.Json,
                OffsetName = model.OffsetName,
                Offsetvalue = model.Offsetvalue
            };

            repository.Execute(criteria);
        }
    }
}
