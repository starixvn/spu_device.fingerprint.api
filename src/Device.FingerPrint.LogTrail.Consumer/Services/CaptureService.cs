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
            var groupMessage = logEventInfos.GroupBy(x => new
            {
                x.EventDate.Date,
                x.SubscriberID,
                x.SystemName,
                x.Source,
                x.Destination,
                x.Method
            }).Select(group => new LogEventInfo
            {
                EventDate = group.Key.Date,
                SubscriberID = group.Key.SubscriberID,
                SystemName = group.Key.SystemName,
                Source = group.Key.Source,
                Destination = group.Key.Destination,
                Method = group.Key.Method,
                Total = group.Count()
            });

            var rawLogEvent = JsonSerializer.Serialize(groupMessage);
            return await logTrailServices.InsertSummaryLogTrailAsync(rawLogEvent);
        }
    }
}
