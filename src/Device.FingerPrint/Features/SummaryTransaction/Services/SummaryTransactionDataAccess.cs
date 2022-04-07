using Device.FingerPrint.Features.SummaryTransaction.Criterias;
using Fanex.Data.Repository;
using System;

namespace Device.FingerPrint.Features.SummaryTransaction.Services
{
    public interface ISummaryTransactionDataAccess
    {
        void SummaryServerTransactionDaily(int batchSize);

        void SummaryClientTransactionDaily(int batchSize);

        void SummaryTransactionDaily(int batchSize);
    }
    public class SummaryTransactionDataAccess : ISummaryTransactionDataAccess
    {
        private readonly IDynamicRepository repository;

        public SummaryTransactionDataAccess()
        {
            repository = new DynamicRepository();
        }

        public void SummaryServerTransactionDaily(int batchSize)
        {
            var criteria = new SummaryServerTransactionDailyCriteria
            {
                BatchSize = batchSize
            };

            repository.Execute(criteria);
        }

        public void SummaryClientTransactionDaily(int batchSize)
        {
            var criteria = new SummaryClientTransactionDailyCriteria
            {
                BatchSize = batchSize
            };

            repository.Execute(criteria);
        }

        public void SummaryTransactionDaily(int batchSize)
        {
            throw new NotImplementedException();
        }
    }
}
