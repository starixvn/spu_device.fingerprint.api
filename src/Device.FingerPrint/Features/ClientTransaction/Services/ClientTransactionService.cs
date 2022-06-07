using Device.FingerPrint.Features.ClientTransaction.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.ClientTransaction.Services
{
    public interface IClientTransactionService
    {
        void InsertClientTransaction(IEnumerable<ClientTransactionEntity> listClientTransaction);

        void InsertFingerprintAttributes(IEnumerable<FingerprintAttributesEntity> listFpAttributes);
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

        public void InsertFingerprintAttributes(IEnumerable<FingerprintAttributesEntity> listFpAttributes)
        {
            string JsonFpAttributes = JsonConvert.SerializeObject(listFpAttributes);

            clientTransactionDataAccess.InsertFingerprintAttributes(JsonFpAttributes);
        }
    }
}
