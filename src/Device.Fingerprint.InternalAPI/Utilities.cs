using Fanex.Logging.Extensions.Database;
using Fanex.Logging.Extensions;

namespace Device.Fingerprint.InternalAPI
{
    public class Utilities
    {

        public static string GetLogMessage(Exception ex)
        {
            return new LogMessage(ex.Message)
                   .AddSection("Database Info", DatabaseInfoGetters.GetDbExceptionInfo(ex))
                   .AddSection("Server Info", CommonInfoGetters.GetServerInfo())
                   .AddSection("Exception Info", CommonInfoGetters.GetExceptionInfo(ex))
                   .ToString();
        }
    }
}
