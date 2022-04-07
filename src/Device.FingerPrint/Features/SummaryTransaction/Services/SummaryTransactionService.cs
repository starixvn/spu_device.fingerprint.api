using System;

namespace Device.FingerPrint.Features.SummaryTransaction.Services
{

    public interface ISummaryTransactionService
    {
        void SummaryServerTransactionDaily(int batchSize);

        void SummaryClientTransactionDaily(int batchSize);

        void SummaryTransactionDaily(int batchSize);
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
            throw new NotImplementedException();
        }
    }
}
