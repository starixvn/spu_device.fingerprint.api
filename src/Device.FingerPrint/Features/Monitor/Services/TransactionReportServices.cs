using Device.FingerPrint.Model;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface ITransactionReportServices
    {
        IEnumerable<ServerTransDailySumModel> GetServerTransDailySumList(string DMYDate);

        IEnumerable<ClientTransDailySumModel> GetClientTransDailySumList(string DMYDate);

        void MonthlySum(string Month);
    }

    public class TransactionReportServices : ITransactionReportServices
    {

        private readonly ITransactionReportDataAccess dataAccess;

        public TransactionReportServices(ITransactionReportDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
        }

        public IEnumerable<ServerTransDailySumModel> GetServerTransDailySumList(string DMYDate)
        {
            var list = dataAccess.GetServerTransDailySumList(DMYDate);

            if (list != null && list.Any())
            {
                return list.Select(x => new ServerTransDailySumModel
                {
                    TransDate = x.TransDate,
                    SubscriberId = x.SubscriberId,
                    SubscriberName = x.SubscriberName,
                    TotalTrans = x.TotalTrans,
                    TotalValidTrans = x.TotalValidTrans
                });
            }

            return new List<ServerTransDailySumModel>();
        }

        public IEnumerable<ClientTransDailySumModel> GetClientTransDailySumList(string DMYDate)
        {
            var list = dataAccess.GetClientTransDailySumList(DMYDate);

            if (list != null && list.Any())
            {
                return list.Select(x => new ClientTransDailySumModel
                {
                    TransDate = x.TransDate,
                    SubscriberId = x.SubscriberId,
                    SubscriberName = x.SubscriberName,
                    TotalTrans = x.TotalTrans,
                    TotalValidTrans = x.TotalValidTrans
                });
            }

            return new List<ClientTransDailySumModel>();
        }

        public void MonthlySum(string Month)
        {
            dataAccess.MonthlySum(Month);
        }
    }
}
