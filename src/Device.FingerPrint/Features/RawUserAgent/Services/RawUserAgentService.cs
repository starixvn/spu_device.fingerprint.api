using Device.FingerPrint.Model;

namespace Device.FingerPrint.Features.RawUserAgent.Services
{

    public interface IRawUserAgentService
    {
        RawUserAgentModel GetUserAgent(int batchSize, int offsetId);
    }

    public class RawUserAgentService : IRawUserAgentService
    {

        private readonly IRawUserAgentDataAccess userAgentDataAccess;

        public RawUserAgentService(IRawUserAgentDataAccess userAgentDataAccess)
        {
            this.userAgentDataAccess = userAgentDataAccess;
        }

        public RawUserAgentModel GetUserAgent(int batchSize, int offsetId)
        {
            var userAgent = userAgentDataAccess.GetUserAgent(batchSize, offsetId);

            if (userAgent != null)
            {
                return new RawUserAgentModel
                {
                    Id = userAgent.Id,
                    UserAgent = userAgent.UserAgent,
                    UserAgentCode = userAgent.UserAgentCode
                };
            }

            return null;
        }
    }
}
