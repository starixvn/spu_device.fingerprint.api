using Device.FingerPrint.Features.Monitor.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportServices
    {
        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionDailySumList(string mdyDate);

        (IEnumerable<TransactionReportEntity>, IEnumerable<TransactionReportEntity>) GetTransactionMonthlySumList(string Month);

        void InsertClientTransactionDailySum(IEnumerable<TransactionSumEntity> data);

        void InsertServerTransactionDailySum(IEnumerable<TransactionSumEntity> data);
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

        public void InsertClientTransactionDailySum(IEnumerable<TransactionSumEntity> data)
        {
            if (data != null && data.Any())
            {
                var json = JsonConvert.SerializeObject(data);

                dataAccess.InsertClientTransactionDailySum(json);
            }
            
        }

        public void InsertServerTransactionDailySum(IEnumerable<TransactionSumEntity> data)
        {
            if(data != null && data.Any())
            {
                var json = JsonConvert.SerializeObject(data);

                dataAccess.InsertServerTransactionDailySum(json);
            }           
        }
    }
}
