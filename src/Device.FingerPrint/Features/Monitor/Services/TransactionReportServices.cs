using Device.FingerPrint.Features.Monitor.Model;
using Device.FingerPrint.Model;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportServices
    {
        (IEnumerable<ServerTransDailySumEntity>, IEnumerable<ClientTransDailySumEntity>) GetServerTransDailySumList(string DMYDate);

        void MonthlySum(string Month);
    }

    public class TransactionReportServices : ITransactionReportServices
    {

        private readonly ITransactionReportDataAccess dataAccess;

        public TransactionReportServices(ITransactionReportDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public (IEnumerable<ServerTransDailySumEntity>, IEnumerable<ClientTransDailySumEntity>) GetServerTransDailySumList(string DMYDate)
        {
            return dataAccess.GetServerTransDailySumList(DMYDate);
        }

        public void MonthlySum(string Month)
        {
            dataAccess.MonthlySum(Month);
        }
    }
}
