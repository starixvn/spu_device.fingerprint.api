using Device.FingerPrint.Model;

namespace Device.FingerPrint.Features.UserAgent.Services
{

    public interface IUserAgentService
    {
        UserAgentModel GetUserAgent(int batchSize, int offsetId);
    }

    public class UserAgentService : IUserAgentService
    {

        private readonly IUserAgentDataAccess userAgentDataAccess;

        public UserAgentService(IUserAgentDataAccess userAgentDataAccess)
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
