using Device.FingerPrint.Features.Monitor.Criterias;
using Device.FingerPrint.Features.Monitor.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportDataAccess
    {
        IEnumerable<ServerTransDailySumEntity> GetServerTransDailySumList(string DMYDate);

        IEnumerable<ClientTransDailySumEntity> GetClientTransDailySumList(string DMYDate);

        void MonthlySum(string Month);
    }

    public class TransactionReportDataAccess : ITransactionReportDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionReportDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<ServerTransDailySumEntity> GetServerTransDailySumList(string DMYDate)
        {
            var criteria = new GetServerTransDailySumCriteria
            {
                DMYDate = DMYDate
            };

            return repository.Fetch<ServerTransDailySumEntity>(criteria);
        }

        public IEnumerable<ClientTransDailySumEntity> GetClientTransDailySumList(string DMYDate)
        {
            var criteria = new GetClientTransDailySumCriteria
            {
                DMYDate = DMYDate
            };

            return repository.Fetch<ClientTransDailySumEntity>(criteria);
        }

        public void MonthlySum(string Month)
        {
            var criteria = new MonthlySumCriteria
            {
                Month = Month
            };

            repository.Execute(criteria);
        }
    }
}
