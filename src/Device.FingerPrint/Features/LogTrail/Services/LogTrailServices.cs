namespace Device.FingerPrint.Features.LogTrail.Services
{
    using System.Threading.Tasks;

    public interface ILogTrailServices
    {
        Task<int> InsertSummaryLogTrailAsync(string rawLogEvent);

        Task<int> InsertLogTrailDetailAsync(string rawLogEvent);
    }
    public class LogTrailServices : ILogTrailServices
    {
        private readonly ILogTrailDataAccess dataAccess;
        public LogTrailServices(ILogTrailDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task<int> InsertSummaryLogTrailAsync(string rawLogEvent)
        {
           return await dataAccess.InsertSummaryLogTrailAsync(rawLogEvent);
        }

        public async Task<int> InsertLogTrailDetailAsync(string rawLogEvent)
        {
            return await dataAccess.InsertLogTrailDetailAsync(rawLogEvent);
        }
    }
}
