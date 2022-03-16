using Device.FingerPrint.Features.RawTransaction.Model;
using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Kafka;
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
        private readonly IRawTransactionService rawTransactionService;
        private readonly List<TransactionEntity> listTrans;
        private readonly ISentryClient sentryClient;
        private readonly IAppSettings appSettings;

        public KafkaHandler(ISentryClient sentryClient, IRawTransactionService rawTransactionService, IAppSettings appSettings)
        {
            this.rawTransactionService = rawTransactionService;
            this.sentryClient = sentryClient;
            this.appSettings = appSettings;

            listTrans = new List<TransactionEntity>();
        }

        public Task HandleAsync(string key, TransactionModel value)
        {
            listTrans.Add(TranformData(value));

            if (listTrans.Count >= appSettings.ConsumerBatchSize)
            {
                try
                {
                    rawTransactionService.InsertServerTransaction(listTrans);
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }

                listTrans.Clear();
            }

            return Task.CompletedTask;
        }

        private static TransactionEntity TranformData(TransactionModel model)
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
