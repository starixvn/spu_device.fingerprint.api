using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
        void InsertListTransaction(List<TransactionModel> transaction);

        void RecoverDevice(int batchSize);
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionDataAccess transactionDataAccess;

        public TransactionService(ITransactionDataAccess transactionDataAccess)
        {
            this.transactionDataAccess = transactionDataAccess;
        }

        public void InsertListTransaction(List<TransactionModel> listTransaction)
        {
            string JsonFingerPrint = JsonConvert.SerializeObject(listTransaction);
            transactionDataAccess.InsertJsonTransaction(JsonFingerPrint);
        }

        public void RecoverDevice(int batchSize)
        {
            transactionDataAccess.RecoverDevice(batchSize);
        }
    }
}
