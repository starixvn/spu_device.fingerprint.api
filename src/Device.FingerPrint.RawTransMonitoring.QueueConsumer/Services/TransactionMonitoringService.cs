namespace Device.FingerPrint.RawTransMonitoring.QueueConsumer.Services
{
    using Device.FingerPrint.Features.MonitorTransaction.Services;
    using Device.FingerPrint.Model.TransactionMonitoring;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Threading.Tasks;

    public interface ITransactionMonitoringService
    {
        Task<int> SumRawTransactionAsync(IEnumerable<TransactionMonitoringModel> rawTrans);
    }

    public class TransactionMonitoringService : ITransactionMonitoringService
    {
        private readonly ITransactionMonitoringDataAccess dataAccess;

        public TransactionMonitoringService(ITransactionMonitoringDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public async Task<int> SumRawTransactionAsync(IEnumerable<TransactionMonitoringModel> rawTrans)
        {
            var model = JsonSerializer.Serialize(rawTrans);

            return await dataAccess.SumRawTransactionAsync(model);
        }
    }
}
