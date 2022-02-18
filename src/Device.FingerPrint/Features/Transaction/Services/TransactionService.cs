using Device.FingerPrint.Features.Transaction.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
        void InsertListTransaction(IEnumerable<TransactionEntity> transactions);

        void DeviceIdentify(int batchSize);

        void MergeTransaction(int batchSize);

        void UpdateTransaction(int batchSize);

        void UpdateTransactionN();
    }

    public class TransactionService : ITransactionService
    {
        private readonly ITransactionDataAccess transactionDataAccess;

        public TransactionService(ITransactionDataAccess transactionDataAccess)
        {
            this.transactionDataAccess = transactionDataAccess;
        }

        public void InsertListTransaction(IEnumerable<TransactionEntity> transactions)
        {
            string JsonFingerPrint = JsonConvert.SerializeObject(transactions);

            transactionDataAccess.InsertJsonTransaction(JsonFingerPrint);
        }

        public void DeviceIdentify(int batchSize)
        {
            transactionDataAccess.DeviceIdentify(batchSize);
        }

        public void MergeTransaction(int batchSize)
        {
            transactionDataAccess.MergeTransaction(batchSize);
        }

        public void UpdateTransaction(int batchSize)
        {
            transactionDataAccess.UpdateTransaction(batchSize);
        }

        public void UpdateTransactionN()
        {
            transactionDataAccess.UpdateTransactionN();
        }
    }
}
