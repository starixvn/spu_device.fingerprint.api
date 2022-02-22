using Device.FingerPrint.Features.Offset.Criterias;
using Device.FingerPrint.Features.Offset.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Offset.Services
{
    public interface IOffsetDataAccess
    {
        IEnumerable<OffsetEntity> GetOffset(string OffsetName);
        void UpdateOffsetValue(string OffsetName, long OffsetValue);
    }

    public class OffsetDataAccess : IOffsetDataAccess
    {
        private readonly IDynamicRepository repository;

        public OffsetDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<OffsetEntity> GetOffset(string OffsetName)
        {
            var criteria = new GetOffsetCriteria
            {
                OffsetName = OffsetName
            };

            return repository.Fetch<OffsetEntity>(criteria);
        }

        public void UpdateOffsetValue(string OffsetName, long OffsetValue)
        {
            var criteria = new UpdateOffsetCriteria
            {
                OffsetName = OffsetName,
                OffsetValue = OffsetValue
            };

            repository.Execute(criteria);
        }
    }
}
