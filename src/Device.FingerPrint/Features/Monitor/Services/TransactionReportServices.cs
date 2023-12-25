namespace Device.FingerPrint.Features.Monitor.Services
{
    using Device.FingerPrint.Features.Monitor.Model;
    using System;
    using System.Collections.Generic;

    public interface ITransactionReportServices
    {
        IEnumerable<TransactionReportEntity> GetTransactionSumList(DateTime fromDate, DateTime toDate);
    }

    public class TransactionReportServices : ITransactionReportServices
    {

        private readonly ITransactionReportDataAccess dataAccess;

        public TransactionReportServices(ITransactionReportDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public IEnumerable<TransactionReportEntity> GetTransactionSumList(DateTime fromDate, DateTime toDate)
        {
            return dataAccess.GetTransactionSumList(fromDate, toDate);
        }
    }
}
