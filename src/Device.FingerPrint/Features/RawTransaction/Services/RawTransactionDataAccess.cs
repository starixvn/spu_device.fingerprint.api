
using Device.FingerPrint.Features.RawTransaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.RawTransaction.Services
{

    public interface IRawTransactionDataAccess
    {
        IEnumerable<RawTransactionEntity> GetRawTrans(int batchSize, int retry, int offsetId);

        IEnumerable<RawTransactionEntity> GetRawTransN(int batchSize, int offsetId);

        void InsertRawTransaction(string transJson);

        void UpdateRawTransaction(string rawTransJson, int retry, int offsetId, long offsetValue);

        void UpdateRawTransactionN(string rawTransJson, int offsetId, long offsetValue);

    }

    public class RawTransactionDataAccess: IRawTransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public RawTransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<RawTransactionEntity> GetRawTrans(int batchSize, int retry, int offsetId)
        {
            var criteria = new GetRawTransactionCriteria 
            { 
                Size = batchSize,
                Retry = retry,
                OffsetId = offsetId
            };

            return repository.Fetch<RawTransactionEntity>(criteria);
        }

        public IEnumerable<RawTransactionEntity> GetRawTransN(int batchSize, int offsetId)
        {
            var criteria = new GetRawTransactionNCriteria 
            { 
                Size = batchSize,
                OffsetId = offsetId
            };

            return repository.Fetch<RawTransactionEntity>(criteria);
        }

        public void InsertRawTransaction(string transJson)
        {
            var criteria = new InsertRawTransactionCriteria
            {
                Transaction = transJson
            };

            repository.Execute(criteria);
        }

        public void UpdateRawTransaction(string rawTransJson, int retry, int offsetId, long offsetValue)
        {
            var criteria = new UpdateRawTransactionCriteria
            {
                RawTransJson = rawTransJson,
                Retry = retry,
                OffsetId = offsetId,
                OffsetValue = offsetValue
            };

            repository.Execute(criteria);
        }

        public void UpdateRawTransactionN(string rawTransJson, int offsetId, long offsetValue)
        {
            var criteria = new UpdateRawTransactionNCriteria
            {
                RawTransJson = rawTransJson,
                OffsetId = offsetId,
                OffsetValue = offsetValue
            };

            repository.Execute(criteria);
        }
    }
}
