using Device.FingerPrint.Features.RiskLevel.Criterias;
using Device.FingerPrint.Features.RiskLevel.Model;
using Device.FingerPrint.Features.Transaction.Criterias;
using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Device.FingerPrint.Features.RiskLevel.Services
{
    public interface IRiskLevelAccess
    {
        IEnumerable<RiskLevelEntity> GetCustomerClassRiskLevel(int subscriberId, long maxId, int batchSize);
    }

    public class RiskLevelDataAccess : IRiskLevelAccess
    {
        private readonly IDynamicRepository repository;

        public RiskLevelDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<RiskLevelEntity> GetCustomerClassRiskLevel(int subscriberId, long maxId, int batchSize)
        {
            var criteria = new GetLatestRiskLevelCriteria
            {
                 SubscriberId = subscriberId,
                 MaxId = maxId,
                 BatchSize = batchSize
            };

            return repository.Fetch<RiskLevelEntity>(criteria);
        }
    }
}
