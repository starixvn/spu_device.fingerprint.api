using Fanex.Data.Repository;
using Device.FingerPrint.Features.FingerPrint.Criterias;

namespace Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintDataAccess
    {
        void InsertClientTransaction(string jsonFingerPrint);
    }

    public class FingerPrintDataAccess : IFingerPrintDataAccess
    {
        private readonly IDynamicRepository repository;

        public FingerPrintDataAccess()
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
