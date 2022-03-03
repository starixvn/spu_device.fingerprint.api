using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;
using System.Collections.Generic;
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

        private static FingerPrintEntity TranformFingerPrint(FingerPrintModel model)
        {
            return new FingerPrintEntity
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
