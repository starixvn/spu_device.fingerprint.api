using Fps.Device.FingerPrint.Features.Transaction.Model;
using FPS.Device.FingerPrint.Model;

namespace Fps.Device.FingerPrint.Features.Transaction.Services
{
    public interface ITransactionService
    {
        void InsertTransaction(TransactionModel transaction);

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

        public void RecoverDevice(int batchSize)
        {
            transactionDataAccess.RecoverDevice(batchSize);
        }
    }
}
