using Device.FingerPrint.Model;
using System.Collections.Generic;
using System.Linq;

namespace Device.FingerPrint.Features.Monitor.Services
{
    public interface IClientTransDailySumServices
    {
        IEnumerable<ClientTransDailySumModel> GetClientTransDailySumList(string DMYDate);
    }

    public class ClientTransDailySumServices : IClientTransDailySumServices
    {

        private readonly IClientTransDailySumDataAccess dataAccess;

        public ClientTransDailySumServices(IClientTransDailySumDataAccess dataAccess)
        {
            this.dataAccess = dataAccess;
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
    }
}
