using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawTransaction.Handlers
{
    public class KafkaHandler : IKafkaHandler<string, TransactionModel>
    {
        private readonly ITransactionService transactionService;
        private readonly List<TransactionEntity> listTrans;

        public KafkaHandler(ITransactionService transactionService)
        {
            this.transactionService = transactionService;

            listTrans = new List<TransactionEntity>();
        }

        public Task HandleAsync(string key, TransactionModel value)
        {
            listTrans.Add(TranformTransaction(value));

            if(listTrans.Count >= 10)
            {
                transactionService.InsertListTransaction(listTrans);

                listTrans.Clear();
            }
            
            return Task.CompletedTask;
        }

        private TransactionEntity TranformTransaction(TransactionModel model)
        {
            return new TransactionEntity
            {
                LoginName = model.LoginName,
                ClientIp = model.ClientIp,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                TransKey = model.TransKey,
                TransTime = model.TransTime,
                Url = model.Url,
                UserAgent = model.UserAgent,
                CustId = model.CustId,
                SubAccId = model.SubAccId
            };
        }
    }
}
