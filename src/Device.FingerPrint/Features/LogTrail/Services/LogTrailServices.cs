namespace Device.FingerPrint.Features.LogTrail.Services
{
    public interface ILogTrailServices
    {
        int InsertSummaryLogTrail(string RawLogEvent);
    }
    public class LogTrailServices : ILogTrailServices
    {
        private readonly ILogTrailDataAccess dataAccess;
        public LogTrailServices(ILogTrailDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public int InsertSummaryLogTrail(string RawLogEvent)
        {
           return dataAccess.InsertSummaryLogTrail(RawLogEvent);
        }
    }
}
