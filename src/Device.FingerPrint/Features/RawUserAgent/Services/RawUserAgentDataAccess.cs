using Device.FingerPrint.Features.RawUserAgent.Criterias;
using Device.FingerPrint.Features.RawUserAgent.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.RawUserAgent.Services
{
    public interface IRawUserAgentDataAccess
    {
        IEnumerable<RawUserAgentEntity> GetRawUserAgents(int batchSize);

        void UpdateRawUserAgent(string rawUserAgentJson);
    }

    public class RawUserAgentDataAccess : IRawUserAgentDataAccess
    {
        private readonly IDynamicRepository repository;

        public RawUserAgentDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<RawUserAgentEntity> GetRawUserAgents(int batchSize)
        {
            var criteria = new GetRawUserAgentCriteria
            {
                Size = batchSize
            };

            return repository.Fetch<RawUserAgentEntity>(criteria);
        }

        public void UpdateRawUserAgent(string rawUserAgentJson)
        {
            var criteria = new UpdateRawUserAgentCriteria
            {
                RawUserAgentJson = rawUserAgentJson
            };

            repository.Execute(criteria);
        }
    }
}
