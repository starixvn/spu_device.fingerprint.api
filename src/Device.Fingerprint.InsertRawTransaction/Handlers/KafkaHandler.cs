using Device.FingerPrint.Features.Transaction.Model;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawTransaction.Handlers
{
    public class KafkaHandler : IKafkaHandler<string, TransactionModel>
    {
        private readonly ITransactionService transactionService;
        private readonly List<TransactionEntity> listTrans;
        private readonly ISentryClient sentryClient;

        public KafkaHandler(ISentryClient sentryClient, ITransactionService transactionService)
        {
            this.transactionService = transactionService;
            this.sentryClient = sentryClient;

            listTrans = new List<TransactionEntity>();
        }

        public Task HandleAsync(string key, TransactionModel value)
        {
            listTrans.Add(TranformTransaction(value));

            if(listTrans.Count >= 1)
            {
                try
                {
                    transactionService.InsertListTransaction(listTrans);
                }
                catch(Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }
                

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
                Action = model.Action,
                UserAgent = model.UserAgent,
                CustId = model.CustId,
                SubAccId = model.SubAccId
            };
        }
    }
}
