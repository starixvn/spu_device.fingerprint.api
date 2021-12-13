using Fanex.Data.Repository;
using Fps.Device.FingerPrint.Features.FingerPrint.Criterias;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Services
{
   public interface IFingerPrintDataAccess
    {
        void InsertFingerPrint(FingerPrintEntity fingerPrint);
    }

    public class FingerPrintDataAccess : IFingerPrintDataAccess
    {
        private readonly IDynamicRepository repository;

        public FingerPrintDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertFingerPrint(FingerPrintEntity fingerPrint)
        {
            var criteria = new InsertFingerPrintCriteria
            {
                FingerPrint = fingerPrint.FingerPrint,
                SessionKey = fingerPrint.SessionKey,
                SubscriberId = fingerPrint.SubscriberId,
                Attributes = fingerPrint.Attributes,
                Device = fingerPrint.Device,
                DeviceCode = fingerPrint.DeviceCode
            };

            repository.Execute(criteria);
        }
    }
}
