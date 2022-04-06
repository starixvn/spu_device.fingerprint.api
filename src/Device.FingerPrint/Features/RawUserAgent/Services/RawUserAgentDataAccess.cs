using Device.FingerPrint.Features.RawUserAgent.Criterias;
using Device.FingerPrint.Features.RawUserAgent.Model;
using Fanex.Data;

namespace Device.FingerPrint.Features.RawUserAgent.Services
{
    public interface IRawUserAgentDataAccess
    {
        RawUserAgentEntity GetUserAgent(int batchSize, int offsetId);
    }

    public class RawUserAgentDataAccess : IRawUserAgentDataAccess
    {
        public RawUserAgentEntity GetUserAgent(int batchSize, int offsetId)
        {
            var criteria = new GetRawUserAgentCriteria
            {
                Size = batchSize,
                OffsetId = offsetId
            };

            using var db = ObjectDbFactory.CreateInstance(criteria.GetSettingKey());
            return db.QueryEntity<RawUserAgentEntity>(criteria);
        }
    }
}
