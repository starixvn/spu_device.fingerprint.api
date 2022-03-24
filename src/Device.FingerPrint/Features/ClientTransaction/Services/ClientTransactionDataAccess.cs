using Fanex.Data.Repository;
using Device.FingerPrint.Features.ClientTransaction.Criterias;

namespace Device.FingerPrint.Features.ClientTransaction.Services
{
    public interface IClientTransactionDataAccess
    {
        void InsertClientTransaction(string jsonFingerPrint);
    }

    public class ClientTransactionDataAccess : IClientTransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public ClientTransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertClientTransaction(string jsonFingerPrint)
        {
            var criteria = new InsertClientTransactionCriteria
            {
                FingerPrint = jsonFingerPrint
            };

            repository.Execute(criteria);
        }
    }
}
