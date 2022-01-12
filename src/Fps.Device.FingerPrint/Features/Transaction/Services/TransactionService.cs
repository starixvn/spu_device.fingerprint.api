using Fps.Device.FingerPrint.Features.Transaction.Model;
using FPS.Device.FingerPrint.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Fps.Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
        void InsertTransaction(TransactionModel transaction);
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

        public void InsertTransaction(TransactionModel transaction)
        {
            transactionDataAccess.InsertTransaction(new TransactionEntity
            {
                LoginName = transaction.LoginName,
                ClientIp = transaction.ClientIp,
                SessionKey = transaction.SessionKey,
                SubscriberId = transaction.SubscriberId,
                TransTime = transaction.TransTime,
                UserAgent = transaction.UserAgent,
                Url = transaction.Url
            });
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
