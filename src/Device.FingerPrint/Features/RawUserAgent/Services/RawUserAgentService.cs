using Device.FingerPrint.Model;

namespace Device.FingerPrint.Features.RawUserAgent.Services
{

    public interface IRawUserAgentService
    {
        UserAgentModel GetUserAgent(int batchSize, int offsetId);
    }

    public class RawUserAgentService : IRawUserAgentService
    {

        private readonly IRawUserAgentDataAccess userAgentDataAccess;

        public RawUserAgentService(IRawUserAgentDataAccess userAgentDataAccess)
        {
            this.userAgentDataAccess = userAgentDataAccess;
        }

        public UserAgentModel GetUserAgent(int batchSize, int offsetId)
        {
            var userAgent = userAgentDataAccess.GetUserAgent(batchSize, offsetId);

            if (userAgent != null)
            {
                return new UserAgentModel
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
