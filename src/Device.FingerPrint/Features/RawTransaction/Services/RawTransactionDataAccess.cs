using Device.FingerPrint.Features.Transaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.RawTransaction.Services
{

    public interface IRawTransactionDataAccess
    {
        IEnumerable<RawTransactionEntity> GetRawTrans(RawTransactionConfigModel config);
    }

    public class RawTransactionDataAccess: IRawTransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public RawTransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<RawTransactionEntity> GetRawTrans(RawTransactionConfigModel config)
        {
            var criteria = new GetRawTransCriteria 
            { 
                Size = config.Size,
                Offset = config.Offset,
                Retry = config.Retry
            };

            return repository.Fetch<RawTransactionEntity>(criteria);
        }
    }
}
