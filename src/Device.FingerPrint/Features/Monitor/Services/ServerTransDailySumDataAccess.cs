using Device.FingerPrint.Features.Monitor.Criterias;
using Device.FingerPrint.Features.Monitor.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Monitor.Services
{

    public interface IServerTransDailySumDataAccess
    {
        IEnumerable<ServerTransDailySumEntity> GetServerTransDailySumList(string DMYDate);
    }

    public class ServerTransDailySumDataAccess : IServerTransDailySumDataAccess
    {
        private readonly IDynamicRepository repository;

        public ServerTransDailySumDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<ServerTransDailySumEntity> GetServerTransDailySumList(string DMYDate)
        {
            var criteria = new ServerTransDailySumCriteria
            {
                DMYDate = DMYDate
            };

            return repository.Fetch<ServerTransDailySumEntity>(criteria);
        }
    }
}
