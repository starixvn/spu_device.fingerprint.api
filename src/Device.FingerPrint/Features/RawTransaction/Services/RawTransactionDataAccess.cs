
using Device.FingerPrint.Features.RawTransaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.RawTransaction.Services
{

    public interface IRawTransactionDataAccess
    {
        IEnumerable<RawTransactionEntity> GetRawTrans(int batchSize, int retry, string offsetName);
        IEnumerable<RawTransactionEntity> GetRawTransN(int batchSize, string offsetName);
    }

    public class RawTransactionDataAccess: IRawTransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public RawTransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<RawTransactionEntity> GetRawTrans(int batchSize, int retry, string offsetName)
        {
            var criteria = new GetRawTransCriteria 
            { 
                Size = batchSize,
                Retry = retry,
                OffsetName = offsetName
            };

            return repository.Fetch<RawTransactionEntity>(criteria);
        }

        public IEnumerable<RawTransactionEntity> GetRawTransN(int batchSize, string offsetName)
        {
            var criteria = new GetRawTransNCriteria 
            { 
                Size = batchSize,
                OffsetName = offsetName
            };

            return repository.Fetch<RawTransactionEntity>(criteria);
        }
    }
}
