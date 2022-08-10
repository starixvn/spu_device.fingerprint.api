using Device.FingerPrint.Model;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.RawUserAgent.Services
{

    public interface IRawUserAgentService
    {
        IEnumerable<RawUserAgentModel> GetRawUserAgents(int batchSize);

        void UpdateRawUserAgent(string rawUserAgentJson);
    }

    public class RawUserAgentService : IRawUserAgentService
    {

        private readonly IRawUserAgentDataAccess userAgentDataAccess;

        public RawUserAgentService(IRawUserAgentDataAccess userAgentDataAccess)
        {
            this.userAgentDataAccess = userAgentDataAccess;
        }

        public IEnumerable<RawUserAgentModel> GetRawUserAgents(int batchSize)
        {
            var userAgent = userAgentDataAccess.GetRawUserAgents(batchSize);

            if (userAgent != null && userAgent.Any())
            {
                return userAgent.Select(x => new RawUserAgentModel
                {
                    Id = x.Id,
                    UserAgent = x.UserAgent,
                    UserAgentCode = x.UserAgentCode
                });
            }

            return new List<RawUserAgentModel>();
        }

        public void UpdateRawUserAgent(string rawUserAgentJson)
        {
            userAgentDataAccess.UpdateRawUserAgent(rawUserAgentJson);
        }
    }
}
