using Device.FingerPrint.Features.FingerPrint.Criterias;
using Device.FingerPrint.Features.FingerPrint.Entities;
using Fanex.Data;
using System;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.FingerPrint.Services
{

    public interface IFingerPrintDataAccess
    {
        IEnumerable<FPFingerPrintEntity> GetFPFingerPrint(DateTime fromDate, DateTime toDate, int batchSize, ulong lastID);
    }

    public class FingerPrintDataAccess : IFingerPrintDataAccess
    {
        public IEnumerable<FPFingerPrintEntity> GetFPFingerPrint(DateTime fromDate, DateTime toDate, int batchSize, ulong lastID)
        {
            using var objectDb = ObjectDbFactory.CreateInstance("DCS_DC_OpenSearch_FPFingerPrint_Get");
            var criteria = new GetFPFingerPrintCriteria
            {
                FromDate = fromDate,
                ToDate = toDate,
                BatchSize = batchSize,
                LastID = lastID
            };

            //using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
            return new List<FPFingerPrintEntity>();
            //return db.Query<FPFingerPrintEntity>(criteria);
        }
    }
}
