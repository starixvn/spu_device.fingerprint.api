namespace Device.FingerPrint.LogTrail.Consumer.Services
{
    using System.Text.Json;
    using Device.FingerPrint.Features.LogTrail.Services;
    using Device.FingerPrint.LogTrail.Model;

    public interface ICapturingService
    {
        int InserLogTrailEvent(IEnumerable<RawLogEventModel> logEventInfos);
    }

    public class CaptureService : ICapturingService
    {
        private readonly ILogTrailServices logTrailServices;

        public CaptureService(ILogTrailServices logTrailServices)
        {
            this.logTrailServices = logTrailServices;
        }

        public int InserLogTrailEvent(IEnumerable<RawLogEventModel> logEventInfos)
        {
            var rawLogEvent = JsonSerializer.Serialize(logEventInfos);
            return logTrailServices.InsertSummaryLogTrail(rawLogEvent);
        }
    }
}
