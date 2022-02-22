using Device.FingerPrint.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Device.FingerPrint.Features.Offset.Services
{
    public interface IOffsetService
    {
        IEnumerable<OffsetModel> GetOffset(string OffsetName);
        void UpdateOffsetValue(string OffsetName, long OffsetValue);
    }

    public class OffsetService : IOffsetService
    {

        private readonly IOffsetDataAccess offsetDataAccess;

        public OffsetService(IOffsetDataAccess offsetDataAccess)
        {
            this.offsetDataAccess = offsetDataAccess;
        }

        public IEnumerable<OffsetModel> GetOffset(string OffsetName)
        {
            var rawTrans = offsetDataAccess.GetOffset(OffsetName);

            if (rawTrans != null && rawTrans.Any())
            {
                return rawTrans.Select(x => new OffsetModel
                {
                    Id = x.Id,
                    OffsetName = x.OffsetName,
                    OffsetValue = x.OffsetValue
                });
            }

            return new List<OffsetModel>();
        }

        public void UpdateOffsetValue(string OffsetName, long OffsetValue)
        {
            offsetDataAccess.UpdateOffsetValue(OffsetName, OffsetValue);
        }
    }
}
