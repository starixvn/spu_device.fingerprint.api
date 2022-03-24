using Device.FingerPrint.Features.Monitor.Model;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportServices
    {
        (IEnumerable<ServerTransDailySumEntity>, IEnumerable<ClientTransDailySumEntity>) GetTransactionDailySumList(string DMYDate);

        (IEnumerable<ServerTransMonthlySumEntity>, IEnumerable<ClientTransMonthlySumEntity>) GetTransactionMonthlySumList(string Month);

        void MonthlySum(string Month);
    }

    public class TransactionReportServices : ITransactionReportServices
    {

        private readonly ITransactionReportDataAccess dataAccess;

        public TransactionReportServices(ITransactionReportDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public (IEnumerable<ServerTransDailySumEntity>, IEnumerable<ClientTransDailySumEntity>) GetTransactionDailySumList(string DMYDate)
        {
            return dataAccess.GetTransactionDailySumList(DMYDate);
        }

        public (IEnumerable<ServerTransMonthlySumEntity>, IEnumerable<ClientTransMonthlySumEntity>) GetTransactionMonthlySumList(string Month)
        {
            return dataAccess.GetTransactionMonthlySumList(Month);
        }

        public void MonthlySum(string Month)
        {
            dataAccess.MonthlySum(Month);
        }
    }
}
