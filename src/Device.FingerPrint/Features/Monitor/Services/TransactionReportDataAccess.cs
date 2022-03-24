using Device.FingerPrint.Features.Monitor.Criterias;
using Device.FingerPrint.Features.Monitor.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportDataAccess
    {
        (IEnumerable<ServerTransDailySumEntity>, IEnumerable<ClientTransDailySumEntity>) GetServerTransDailySumList(string DMYDate);

        void MonthlySum(string Month);
    }

    public class TransactionReportDataAccess : ITransactionReportDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionReportDataAccess()
        {
            repository = new DynamicRepository();
        }

        public (IEnumerable<ServerTransDailySumEntity>, IEnumerable<ClientTransDailySumEntity>) GetServerTransDailySumList(string DMYDate)
        {
            var criteria = new GetTransactionDailySumCriteria
            {
                DMYDate = DMYDate
            };

            return repository.FetchMultiple<ServerTransDailySumEntity, ClientTransDailySumEntity>(criteria);
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
