using Fanex.Data.Repository;
using Device.FingerPrint.Features.ClientTransaction.Criterias;

namespace Device.FingerPrint.Features.ClientTransaction.Services
{
    public interface IClientTransactionDataAccess
    {
        void InsertClientTransactionException(string function, string exception, string userAgent);
    }

    public class ClientTransactionDataAccess : IClientTransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public ClientTransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertClientTransactionException(string function, string exception, string userAgent)
        {
            var criteria = new InsertClientTransactionExceptionCriteria
            {
                Function = function,
                Exception = exception,
                UserAgent = userAgent
            };

            repository.Execute(criteria);
        }
    }
}
