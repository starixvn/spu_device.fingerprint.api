namespace Device.FingerPrint.LogTrail.Consumer.Services
{
    using System.Text.Json;
    using System.Threading.Tasks;
    using Device.FingerPrint.Core.Model.LogEvent;
    using Device.FingerPrint.Features.LogTrail.Services;

    public interface ICapturingService
    {
        Task<int> InsertLogTrailEventAsync(IEnumerable<LogEventInfo> logEventInfos);

        Task<int> InsertLogTrailEventDetailAsync(IEnumerable<LogEventInfo> logEventInfos);
    }

    public class CaptureService : ICapturingService
    {
        private readonly ILogTrailServices logTrailServices;

        public CaptureService(ILogTrailServices logTrailServices)
        {
            this.logTrailServices = logTrailServices;
        }

        public async Task<int> InsertLogTrailEventAsync(IEnumerable<LogEventInfo> logEventInfos)
        {
            var logEventSummary = logEventInfos.GroupBy(x => new { x.EventCode })
                                    .Select(group => new LogEventEntity
                                    {
                                        EventDate = group.First().EventDate,
                                        SubscriberID = group.First().SubscriberID,
                                        EventCode = group.Key.EventCode,
                                        SystemName = group.First().SystemName,
                                        Source = group.First().Source,
                                        Destination = group.First().Destination,
                                        Method = group.First().Method,
                                        Total = group.Count()
                                    });

            var rawLogEvent = JsonSerializer.Serialize(logEventSummary);

            return await logTrailServices.InsertSummaryLogTrailAsync(rawLogEvent);
        }

        public async Task<int> InsertLogTrailEventDetailAsync(IEnumerable<LogEventInfo> logEventInfos)
        {
            var logEventDetails = logEventInfos.GroupBy(x => new { x.EventCodeDetail })
                                    .Select(group => new LogEventDetail
                                    {
                                        EventDate = group.First().EventDate,
                                        SubscriberID = group.First().SubscriberID,
                                        LoginName = group.First().LoginName,
                                        TransTime = group.First().TransTime,
                                        EventCode = group.Key.EventCodeDetail,
                                        SystemName = group.First().SystemName,
                                        Source = group.First().Source,
                                        Destination = group.First().Destination,
                                        Method = group.First().Method,
                                        Total = group.Count()
                                    });

            var rawLogEvent = JsonSerializer.Serialize(logEventDetails);

            return await logTrailServices.InsertLogTrailDetailAsync(rawLogEvent);
        }
    }
}
