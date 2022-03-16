using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Kafka;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;
using Sentry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawFingerprint.Handlers
{
    public class KafkaHandler : IKafkaHandler<string, FingerPrintModel>
    {
        private readonly IFingerPrintService fingerPrintService;
        private readonly ISentryClient sentryClient;
        private readonly IAppSettings appSettings;

        private readonly List<ClientTransactionEntity> listTrans;

        public KafkaHandler(ISentryClient sentryClient, IFingerPrintService fingerPrintService, IAppSettings appSettings)
        {
            this.fingerPrintService = fingerPrintService;
            this.sentryClient = sentryClient;
            this.appSettings = appSettings;

            listTrans = new List<ClientTransactionEntity>();
        }

        public Task HandleAsync(string key, FingerPrintModel value)
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

        private static ClientTransactionEntity TranformData(FingerPrintModel model)
        {
            return new ClientTransactionEntity
            {
                Attribute = model.Attribute,
                CookieCode = model.CookieCode,
                UserAgentAttribute = model.UserAgentAttribute,
                UserAgentCode = model.UserAgentCode,
                FingerPrintCode = model.FingerPrintCode,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                Flagged = model.Flagged,
                Url = model.Url
            };
        }
    }
}
