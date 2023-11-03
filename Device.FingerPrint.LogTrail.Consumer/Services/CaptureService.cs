namespace Device.FingerPrint.LogTrail.Consumer.Services
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Device.FingerPrint.Features.LogTrail.Services;
    using FPS.Device.FingerPrint.LogTrail.Net.Standard.Client.Models;

    public interface ICapturingService
    {
        Task<int> InserLogTrailEventAsync(IEnumerable<LogEventInfo> logEventInfos);
    }

    public class CaptureService : ICapturingService
    {
        private readonly ILogTrailServices logTrailServices;

        public CaptureService(ILogTrailServices logTrailServices)
        {
            this.logTrailServices = logTrailServices;
        }

        public async Task<int> InserLogTrailEventAsync(IEnumerable<LogEventInfo> logEventInfos)
        {
            var rawLogEvent = JsonSerializer.Serialize(logEventInfos);
            return await logTrailServices.InsertSummaryLogTrailAsync(rawLogEvent);
        }
    }
}
