using Fanex.Data.Repository;
using Device.FingerPrint.Features.FingerPrint.Criterias;
using Device.FingerPrint.Features.FingerPrint.Model;

namespace Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintDataAccess
    {
        void InsertClientTransaction(string jsonFingerPrint);

        void UpdateFingerPrintCookie(FingerPrintCookieEntity fingerprintCookie);
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

        public void UpdateFingerPrintCookie(FingerPrintCookieEntity fingerprintCookie)
        {
            var criteria = new UpdateFingerPrintCookieCriteria
            {
               CookieId = fingerprintCookie.CookieId,               
               FingerPrint = fingerprintCookie.FingerPrint
            };

            repository.Execute(criteria);            
        }
    }
}
