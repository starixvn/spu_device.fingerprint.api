using Fanex.Data.Repository;
using Fps.Device.FingerPrint.Features.FingerPrint.Criterias;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Services
{
    public interface IFingerPrintDataAccess
    {
        void InsertJsonFingerPrint(string jsonFingerPrint);

        long UpdateCookie(string code);

        void UpdateFingerPrintCookie(FingerPrintCookieEntity fingerprintCookie);
    }

    public class FingerPrintDataAccess : IFingerPrintDataAccess
    {
        private readonly IDynamicRepository repository;

        public FingerPrintDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertJsonFingerPrint(string jsonFingerPrint)
        {
            var criteria = new InsertJsonFingerPrintCriteria
            {
                FingerPrint = jsonFingerPrint
            };

            repository.Execute(criteria);
        }

        public long UpdateCookie(string code)
        {
            var criteria = new UpdateCookieCriteria
            {
                Code = code,
                Id = 0
            };

            repository.Execute(criteria);

            return criteria.Id;
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
