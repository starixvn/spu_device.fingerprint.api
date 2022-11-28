using Device.FingerPrint.Features.SummaryTransaction.Services;
using System.Threading.Tasks;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ISummaryJobServices
    {
        Task<bool> SummaryTransactionDaily(int batchSize);

        Task<bool> SummaryRecoverTypeDaily(int batchSize);
    }

    public class SummaryJobServices : ISummaryJobServices
    {
        private readonly ISummaryTransactionService summaryTransactionService;

        public SummaryJobServices(ISummaryTransactionService summaryTransactionService)
        {
            this.summaryTransactionService = summaryTransactionService;
        }

        public Task<bool> SummaryTransactionDaily(int batchSize)
        {
            summaryTransactionService.SummaryTransactionDaily(batchSize);

            return Task.FromResult(true);
        }

        public Task<bool> SummaryRecoverTypeDaily(int batchSize)
        {
            summaryTransactionService.SummaryRecoverTypeDaily(batchSize);

            return Task.FromResult(true);
        }
    }
}
