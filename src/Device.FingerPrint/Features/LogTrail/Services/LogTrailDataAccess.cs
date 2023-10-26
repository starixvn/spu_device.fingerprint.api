
namespace Device.FingerPrint.Features.LogTrail.Services
{
    using Device.FingerPrint.Features.LogTrail.Criterias;
    using Fanex.Data.Repository;

    public interface ILogTrailDataAccess
    {
        int InsertSummaryLogTrail(string RawLogEvent);
    }
    public class LogTrailDataAccess : ILogTrailDataAccess
    {
        private readonly IDynamicRepository repository;

        public LogTrailDataAccess()
        {
            repository = new DynamicRepository();
        }

        public int InsertSummaryLogTrail(string RawLogEvent)
        {
            var criteria = new InsertSummaryLogTrailCriteria
            {
                RawLogEvent = RawLogEvent
            };

            return repository.Execute(criteria);
        }
    }
}
