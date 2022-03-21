using Device.FingerPrint.Features.ClientTransaction.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.ClientTransaction.Services
{
    public interface IClientTransactionService
    {
        void InsertClientTransaction(IEnumerable<ClientTransactionEntity> listClientTransaction);
    }

    public class ClientTransactionService : IClientTransactionService
    {
        private readonly IClientTransactionDataAccess clientTransactionDataAccess;

        public ClientTransactionService(IClientTransactionDataAccess clientTransactionDataAccess)
        {
            this.clientTransactionDataAccess = clientTransactionDataAccess;
        }

        public void InsertClientTransaction(IEnumerable<ClientTransactionEntity> listClientTransaction)
        {
            string JsonClientTransaction = JsonConvert.SerializeObject(listClientTransaction);

            clientTransactionDataAccess.InsertClientTransaction(JsonClientTransaction);
        }
    }
}
