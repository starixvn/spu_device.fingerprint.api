using System;

namespace Device.FingerPrint.Features.SummaryTransaction.Services
{

    public interface ISummaryTransactionService
    {
        void SummaryServerTransactionDaily(int batchSize);

        void SummaryClientTransactionDaily(int batchSize);

        void SummaryTransactionDaily(int batchSize);

        void SummaryTransactionMonthly();
    }
    public class SummaryTransactionService : ISummaryTransactionService
    {
        private readonly ISummaryTransactionDataAccess summaryTransactionDataAccess;

        public SummaryTransactionService(ISummaryTransactionDataAccess summaryTransactionDataAccess)
        {
            this.summaryTransactionDataAccess = summaryTransactionDataAccess;
        }

        public void SummaryServerTransactionDaily(int batchSize)
        {
            summaryTransactionDataAccess.SummaryServerTransactionDaily(batchSize);
        }

        public void SummaryClientTransactionDaily(int batchSize)
        {
            summaryTransactionDataAccess.SummaryClientTransactionDaily(batchSize);
        }

        public void SummaryTransactionDaily(int batchSize)
        {
            summaryTransactionDataAccess.SummaryTransactionDaily(batchSize);
        }

        public void SummaryTransactionMonthly()
        {
            summaryTransactionDataAccess.SummaryTransactionMonthly();
        }
    }
}
