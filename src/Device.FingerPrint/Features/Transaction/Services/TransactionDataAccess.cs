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
        void InsertTransaction(string transJson);

        void DeviceIdentify(string transJson, long groupId);

        void DeviceIdentifyNoFingerprint(string rawTransIds);

        (IEnumerable<DeviceIdentifyEntity>, IEnumerable<long>) GetDeviceIdentify(int batchSize);

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

        public void InsertTransaction(string transJson)
        {
            var criteria = new InsertTransactionCriteria
            {
                Transaction = transJson
            };

            repository.Execute(criteria);
        }

        public void DeviceIdentify(string transJson, long groupId)
        {
            var criteria = new DeviceIdentifyCriteria
            {
                TransJson = transJson,
                GroupId = groupId
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

        public (IEnumerable<DeviceIdentifyEntity>, IEnumerable<long>) GetDeviceIdentify(int batchSize)
        {
            var criteria = new DeviceIdentifyGetCriteria
            {
                Size = batchSize
            };

            return repository.FetchMultiple<DeviceIdentifyEntity, long>(criteria);
        }

        public void MergeTransaction(int batchSize)
        {
            var criteria = new MergeTransactionCriteria
            {
                Size = batchSize
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
