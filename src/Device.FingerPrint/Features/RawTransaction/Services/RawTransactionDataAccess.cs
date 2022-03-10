
using Device.FingerPrint.Features.RawTransaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.RawTransaction.Services
{

    public interface IRawTransactionDataAccess
    {
        IEnumerable<ServerTransactionEntity> GetServerTrans(int batchSize, int retry, int offsetId);

        IEnumerable<ServerTransactionEntity> GetRawTransN(int batchSize, int offsetId);

        void InsertServerTransaction(string transJson);

        void UpdateServerTransaction(string rawTransJson, int retry, int offsetId, long offsetValue);

        void UpdateServerTransactionN(string rawTransJson, int offsetId, long offsetValue);

    }

    public class RawTransactionDataAccess: IRawTransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public RawTransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<ServerTransactionEntity> GetServerTrans(int batchSize, int retry, int offsetId)
        {
            var criteria = new GetServerTransactionCriteria 
            { 
                Size = batchSize,
                Retry = retry,
                OffsetId = offsetId
            };

            return repository.Fetch<ServerTransactionEntity>(criteria);
        }

        public IEnumerable<ServerTransactionEntity> GetRawTransN(int batchSize, int offsetId)
        {
            var criteria = new GetServerTransactionNCriteria 
            { 
                Size = batchSize,
                OffsetId = offsetId
            };

            return repository.Fetch<ServerTransactionEntity>(criteria);
        }

        public void InsertServerTransaction(string transJson)
        {
            var criteria = new InsertServerTransactionCriteria
            {
                Transaction = transJson
            };

            repository.Execute(criteria);
        }

        public void UpdateServerTransaction(string rawTransJson, int retry, int offsetId, long offsetValue)
        {
            var criteria = new UpdateServerTransactionCriteria
            {
                RawTransJson = rawTransJson,
                Retry = retry,
                OffsetId = offsetId,
                OffsetValue = offsetValue
            };

            repository.Execute(criteria);
        }

        public void UpdateServerTransactionN(string rawTransJson, int offsetId, long offsetValue)
        {
            var criteria = new UpdateServerTransactionNCriteria
            {
                RawTransJson = rawTransJson,
                OffsetId = offsetId,
                OffsetValue = offsetValue
            };

            repository.Execute(criteria);
        }
    }
}
