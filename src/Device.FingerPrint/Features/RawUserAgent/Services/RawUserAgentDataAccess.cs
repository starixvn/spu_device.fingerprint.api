using Device.FingerPrint.Features.RawUserAgent.Criterias;
using Device.FingerPrint.Features.RawUserAgent.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.RawUserAgent.Services
{
    public interface IRawUserAgentDataAccess
    {
        IEnumerable<RawUserAgentEntity> GetRawUserAgents(int batchSize, int offsetId);

        void UpdateRawUserAgent(string rawUserAgentJson, int offsetId, long offsetValue);
    }

    public class RawUserAgentDataAccess : IRawUserAgentDataAccess
    {
        private readonly IDynamicRepository repository;

        public RawUserAgentDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<RawUserAgentEntity> GetRawUserAgents(int batchSize, int offsetId)
        {
            var criteria = new GetRawUserAgentCriteria
            {
                Size = batchSize,
                OffsetId = offsetId
            };

            return repository.Fetch<RawUserAgentEntity>(criteria);
        }

        public void UpdateRawUserAgent(string rawUserAgentJson, int offsetId, long offsetValue)
        {
            var criteria = new UpdateRawUserAgentCriteria
            {
                RawUserAgentJson = rawUserAgentJson,
                OffsetId = offsetId,
                OffsetValue = offsetValue
            };

            repository.Execute(criteria);
        }
    }
}
