using Device.FingerPrint.Features.Monitor.Criterias;
using Device.FingerPrint.Features.Monitor.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportDataAccess
    {
        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionDailySumList(string mdyDate);

        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionMonthlySumList(string Month);
    }

    public class TransactionReportDataAccess : ITransactionReportDataAccess
    {
        private readonly IDynamicRepository repository;

        public TransactionReportDataAccess()
        {
            repository = new DynamicRepository();
        }

        public (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionDailySumList(string mdyDate)
        {
            var criteria = new GetTransactionDailySumCriteria
            {
                MDYDate = mdyDate
            };

            return repository.FetchMultiple<TransactionReportEntity, TransactionReportEntity>(criteria);
        }

        public (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionMonthlySumList(string Month)
        {
            var criteria = new GetTransactionMonthlySumCriteria
            {
                Month = Month
            };

            return repository.FetchMultiple<TransactionReportEntity, TransactionReportEntity>(criteria);
        }
    }
}
