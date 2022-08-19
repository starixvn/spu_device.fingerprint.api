using Device.FingerPrint.Features.Monitor.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportServices
    {
        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionDailySumList(string mdyDate);

        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionMonthlySumList(string Month);

        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionSumList(DateTime fromDate, DateTime toDate);

        void InsertClientTransactionDailySum(string json);

        void InsertServerTransactionDailySum(string json);
    }

    public class TransactionReportServices : ITransactionReportServices
    {

        private readonly ITransactionReportDataAccess dataAccess;

        public TransactionReportServices(ITransactionReportDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionDailySumList(string mdyDate)
        {
            return dataAccess.GetTransactionDailySumList(mdyDate);
        }

        public (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionMonthlySumList(string Month)
        {
            return dataAccess.GetTransactionMonthlySumList(Month);
        }

        public (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionSumList(DateTime fromDate, DateTime toDate)
        {
            return dataAccess.GetTransactionSumList(fromDate, toDate);
        }

        public void InsertClientTransactionDailySum(string json)
        {
            dataAccess.InsertClientTransactionDailySum(json);
        }

        public void InsertServerTransactionDailySum(string json)
        {
            dataAccess.InsertServerTransactionDailySum(json);
        }
    }
}
