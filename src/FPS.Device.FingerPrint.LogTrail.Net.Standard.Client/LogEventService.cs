namespace FPS.Device.FingerPrint.LogTrail.Net.Standard.Client
{
    using System.Threading.Tasks;
    using global::Device.FingerPrint.Core.Model;
    using global::Device.FingerPrint.Core.Services;

    public interface ILogEventService
    {
        Task PostAsync(LogEventInfo logEventInfo);
    }

    public class LogEventService : ILogEventService
    {
        private readonly string LogTrailEndPoint;

        public LogEventService(string LogTrailEndPoint)
        {
            this.LogTrailEndPoint = LogTrailEndPoint;
        }

        public async Task PostAsync(LogEventInfo logEventInfo)
        {

            var logTrailClientService = new LogTrailClientService(LogTrailEndPoint);

            await logTrailClientService.PostAsync(logEventInfo);
        }
    }
}
