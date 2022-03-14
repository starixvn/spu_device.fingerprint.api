using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Features.FingerPrint.Services;
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
        private readonly List<ClientTransactionEntity> listTrans;
        private readonly ISentryClient sentryClient;

        public KafkaHandler(ISentryClient sentryClient, IFingerPrintService fingerPrintService)
        {
            this.fingerPrintService = fingerPrintService;
            this.sentryClient = sentryClient;

            listTrans = new List<ClientTransactionEntity>();
        }

        public Task HandleAsync(string key, FingerPrintModel value)
        {
            listTrans.Add(TranformData(value));

            if(listTrans.Count >= 1)
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
