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
        private readonly ILogTrailClientService logTrailClientService;

        public LogEventService(ILogTrailClientService logTrailClientService)
        {
            this.logTrailClientService = logTrailClientService;
        }

        public async Task PostAsync(LogEventInfo logEventInfo)
        {
            await logTrailClientService.PostAsync(logEventInfo);
        }
    }
}
