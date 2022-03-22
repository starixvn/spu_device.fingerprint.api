using Device.FingerPrint.Features.Monitor.Criterias;
using Device.FingerPrint.Features.Monitor.Model;
using Fanex.Data.Repository;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Monitor.Services
{
    public interface IClientTransDailySumDataAccess
    {
        IEnumerable<ClientTransDailySumEntity> GetClientTransDailySumList(string DMYDate);
    }

    public class ClientTransDailySumDataAccess : IClientTransDailySumDataAccess
    {
        private readonly IDynamicRepository repository;

        public ClientTransDailySumDataAccess()
        {
            repository = new DynamicRepository();
        }

        public IEnumerable<ClientTransDailySumEntity> GetClientTransDailySumList(string DMYDate)
        {
            var criteria = new ClientTransDailySumCriteria
            {
                DMYDate = DMYDate
            };

            return repository.Fetch<ClientTransDailySumEntity>(criteria);
        }
    }
}
