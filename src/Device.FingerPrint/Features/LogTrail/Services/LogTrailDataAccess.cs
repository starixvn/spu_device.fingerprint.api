
namespace Device.FingerPrint.Features.LogTrail.Services
{
    using System.Threading.Tasks;
    using Device.FingerPrint.Features.LogTrail.Criteria;
    using Fanex.Data.Repository;

    public interface ILogTrailDataAccess
    {
        Task<int> InsertSummaryLogTrailAsync(string rawLogEvent);

        Task<int> InsertLogTrailDetailAsync(string rawLogEvent);
    }
    public class LogTrailDataAccess : ILogTrailDataAccess
    {
        private readonly IDynamicRepository repository;

        public LogTrailDataAccess()
        {
            repository = new DynamicRepository();
        }

        public async Task<int> InsertSummaryLogTrailAsync(string rawLogEvent)
        {
            var criteria = new InsertSummaryLogTrailCriteria
            {
                RawLogEvent = rawLogEvent
            };

            return await repository.ExecuteAsync(criteria);
        }

        public async Task<int> InsertLogTrailDetailAsync(string rawLogEvent)
        {
            var criteria = new InsertLogTrailDetailCriteria
            {
                RawLogEvent = rawLogEvent
            };

            return await repository.ExecuteAsync(criteria);
        }
    }
}
