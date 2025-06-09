using Fanex.Logging.Extensions;

namespace Device.Fingerprint.InternalAPI
{
    public class Utilities
    {

        public static string GetLogMessage(Exception ex)
        {
            return new LogMessage(ex.Message)
                   .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                   .ToString();
        }
    }
}
