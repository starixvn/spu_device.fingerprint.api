using Fanex.Data.Repository;
using Device.FingerPrint.Features.ClientTransaction.Criterias;

namespace Device.FingerPrint.Features.ClientTransaction.Services
{
    public interface IClientTransactionDataAccess
    {
        void InsertClientTransaction(string jsonFingerPrint);

        void InsertClientTransactionLogs(string jsonFingerPrint);

        void InsertFingerprintAttributes(string jsonFpAttributes);
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

        public void InsertClientTransactionLogs(string jsonFingerPrint)
        {
            var criteria = new InsertClientTransactionLogsCriteria
            {
                FingerPrint = jsonFingerPrint
            };

            repository.Execute(criteria);
        }

        public void InsertFingerprintAttributes(string jsonFpAttributes)
        {
            var criteria = new InsertFingerprintAttributesCriteria
            {
                FingerprintAttributes = jsonFpAttributes
            };

            repository.Execute(criteria);
        }
    }
}
