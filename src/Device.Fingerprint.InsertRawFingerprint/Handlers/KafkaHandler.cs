using Device.FingerPrint.Features.ClientTransaction.Model;
using Device.FingerPrint.Features.ClientTransaction.Services;
using Device.FingerPrint.Kafka;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawFingerprint.Handlers
{
    public class KafkaHandler : IKafkaHandler<string, ClientTransactionModel>
    {
        private readonly IClientTransactionService fingerPrintService;
        private readonly ISentryClient sentryClient;
        private readonly IAppSettings appSettings;

        private readonly List<ClientTransactionEntity> listTrans;

        public KafkaHandler(ISentryClient sentryClient, IClientTransactionService fingerPrintService, IAppSettings appSettings)
        {
            this.fingerPrintService = fingerPrintService;
            this.sentryClient = sentryClient;
            this.appSettings = appSettings;

            listTrans = new List<ClientTransactionEntity>();
        }

        public Task HandleAsync(string key, ClientTransactionModel value)
        {
            listTrans.Add(TranformData(value));

            if (listTrans.Count >= appSettings.ConsumerBatchSize)
            {
                try
                {
                    fingerPrintService.InsertClientTransaction(listTrans);
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }

                listTrans.Clear();
            }

            return Task.CompletedTask;
        }

        private static ClientTransactionEntity TranformData(ClientTransactionModel model)
        {
            return new ClientTransactionEntity
            {
                Attribute = model.Attribute,
                CookieCode = model.CookieCode,
                FingerPrintCode = model.FingerPrintCode,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                Flagged = model.Flagged,
                Url = model.Url,
                TransTime = model.TransTime
            };
        }
    }
}
