using Device.FingerPrint.Model;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface IServerTransDailySumServices
    {
        IEnumerable<ServerTransDailySumModel> GetServerTransDailySumList(string DMYDate);
    }

    public class ServerTransDailySumServices : IServerTransDailySumServices
    {

        private readonly IServerTransDailySumDataAccess dataAccess;

        public ServerTransDailySumServices(IServerTransDailySumDataAccess dataAccess)
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
    }
}
