using Fanex.Data.Repository;
using Fps.Device.FingerPrint.Features.FingerPrint.Criterias;

namespace Fps.Device.FingerPrint.Features.FingerPrint.Services
{
   public interface IFingerPrintDataAccess
    {
        void InsertFingerPrint(string deviceIdentify, long attributeId);
    }

    public class FingerPrintDataAccess : IFingerPrintDataAccess
    {
        private readonly IDynamicRepository repository;

        public FingerPrintDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void InsertFingerPrint(string deviceIdentify, long attributeId)
        {
            var criteria = new InsertFingerPrintCriteria
            {
                DeviceIndentify = deviceIdentify,
                AttributeId = attributeId
            };

            repository.Execute(criteria);
        }
    }
}
