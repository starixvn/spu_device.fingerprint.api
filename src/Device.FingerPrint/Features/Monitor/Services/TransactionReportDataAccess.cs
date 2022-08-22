using Device.FingerPrint.Features.Monitor.Criterias;
using Device.FingerPrint.Features.Monitor.Model;
using Fanex.Data.Repository;
using System;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportDataAccess
    {
        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionDailySumList(string mdyDate);

        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionMonthlySumList(string Month);

        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionSumList(DateTime fromDate, DateTime toDate);

        void InsertClientTransactionDailySum(string json);

        void InsertServerTransactionDailySum(string json);
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

        public (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionSumList(DateTime fromDate, DateTime toDate)
        {
            var criteria = new GetTransactionSumCriteria
            {
                FromDate = fromDate,
                ToDate = toDate
            };

            return repository.FetchMultiple<TransactionReportEntity, TransactionReportEntity, TransactionReportEntity>(criteria);
        }

        public void InsertClientTransactionDailySum(string json)
        {
            var criteria = new InsertClientTransactionDailySumCriteria
            {
                Data = json
            };

            repository.Execute(criteria);
        }

        public void InsertServerTransactionDailySum(string json)
        {
            var criteria = new InsertServerTransactionDailySumCriteria
            {
                Data = json
            };

            repository.Execute(criteria);
        }
    }
}
