using Device.FingerPrint.Model;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.RawUserAgent.Services
{

    public interface IRawUserAgentService
    {
        IEnumerable<RawUserAgentModel> GetRawUserAgents(int batchSize, int offsetId);

        void UpdateRawUserAgent(string rawUserAgentJson, int offsetId, long offsetValue);
    }

    public class RawUserAgentService : IRawUserAgentService
    {

        private readonly IRawUserAgentDataAccess userAgentDataAccess;

        public RawUserAgentService(IRawUserAgentDataAccess userAgentDataAccess)
        {
            this.userAgentDataAccess = userAgentDataAccess;
        }

        public IEnumerable<RawUserAgentModel> GetRawUserAgents(int batchSize, int offsetId)
        {
            var userAgent = userAgentDataAccess.GetRawUserAgents(batchSize, offsetId);

            if (userAgent != null && userAgent.Any())
            {
                return userAgent.Select(x => new RawUserAgentModel
                {
                    Id = x.Id,
                    UserAgent = x.UserAgent,
                    UserAgentCode = x.UserAgentCode
                });
            }

            return null;
        }

        public void UpdateRawUserAgent(string rawUserAgentJson, int offsetId, long offsetValue)
        {
            userAgentDataAccess.UpdateRawUserAgent(rawUserAgentJson, offsetId, offsetValue);
        }
    }
}
