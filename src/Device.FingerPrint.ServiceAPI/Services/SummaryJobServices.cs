using Device.FingerPrint.Features.SummaryTransaction.Services;

namespace Device.FingerPrint.ServiceAPI.Services
{
    public interface ISummaryJobServices
    {
        void SummaryServerTransactionDaily(int batchSize);

        void SummaryClientTransactionDaily(int batchSize);

        void SummaryTransactionDaily(int batchSize);
    }

    public class SummaryJobServices : ISummaryJobServices
    {
        private readonly ISummaryTransactionService summaryTransactionService;

        public SummaryJobServices(ISummaryTransactionService summaryTransactionService)
        {
            this.summaryTransactionService = summaryTransactionService;
        }

        public void SummaryServerTransactionDaily(int batchSize)
        {
            summaryTransactionService.SummaryServerTransactionDaily(batchSize);
        }

        public void SummaryClientTransactionDaily(int batchSize)
        {
            summaryTransactionService.SummaryClientTransactionDaily(batchSize);
        }

        public void SummaryTransactionDaily(int batchSize)
        {
            summaryTransactionService.SummaryTransactionDaily(batchSize);
        }
    }
}
