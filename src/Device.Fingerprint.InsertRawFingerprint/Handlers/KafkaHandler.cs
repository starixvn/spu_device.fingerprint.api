using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Features.Transaction.Services;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Fingerprint.InsertRawFingerprint.Handlers
{
    public class KafkaHandler : IKafkaHandler<string, FingerPrintModel>
    {
        private readonly IFingerPrintService fingerPrintService;
        private readonly List<FingerPrintEntity> listFingerPrint;

        public KafkaHandler(IFingerPrintService fingerPrintService)
        {
            this.fingerPrintService = fingerPrintService;

            listFingerPrint = new List<FingerPrintEntity>();
        }

        public Task HandleAsync(string key, FingerPrintModel value)
        {
            listFingerPrint.Add(TranformFingerPrint(value));

            if(listFingerPrint.Count >= 1)
            {
                fingerPrintService.InsertListFingerPrint(listFingerPrint);

                listFingerPrint.Clear();
            }
            
            return Task.CompletedTask;
        }

        private FingerPrintEntity TranformFingerPrint(FingerPrintModel model)
        {
            return new FingerPrintEntity
            {
                Attributes = model.Attributes,
                CookieCode = model.CookieCode,
                UserAgent = model.UserAgent,
                UserAgentCode = model.UserAgentCode,
                FingerPrint = model.FingerPrint,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                Flagged = model.Flagged,
                Url = model.Url
            };
        }
    }
}
