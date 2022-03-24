using Device.FingerPrint.Features.Monitor.Criterias;
using Device.FingerPrint.Features.Monitor.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportDataAccess
    {
        (IEnumerable<ServerTransDailySumEntity>, IEnumerable<ClientTransDailySumEntity>) GetTransactionDailySumList(string DMYDate);

        (IEnumerable<ServerTransMonthlySumEntity>, IEnumerable<ClientTransMonthlySumEntity>) GetTransactionMonthlySumList(string DMYDate);

        void MonthlySum(string Month);
    }

    public class TransactionReportDataAccess : ITransactionReportDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionReportDataAccess()
        {
            repository = new DynamicRepository();
        }

        public (IEnumerable<ServerTransDailySumEntity>, IEnumerable<ClientTransDailySumEntity>) GetTransactionDailySumList(string DMYDate)
        {
            var criteria = new GetTransactionDailySumCriteria
            {
                DMYDate = DMYDate
            };

            return repository.FetchMultiple<ServerTransDailySumEntity, ClientTransDailySumEntity>(criteria);
        }

        public (IEnumerable<ServerTransMonthlySumEntity>, IEnumerable<ClientTransMonthlySumEntity>) GetTransactionMonthlySumList(string Month)
        {
            var criteria = new GetTransactionMonthlySumCriteria
            {
                Month = Month
            };

            return repository.FetchMultiple<ServerTransMonthlySumEntity, ClientTransMonthlySumEntity>(criteria);
        }

        public void MonthlySum(string Month)
        {
            var criteria = new TransactionMonthlySumCriteria
            {
                Month = Month
            };

            repository.Execute(criteria);
        }
    }
}
