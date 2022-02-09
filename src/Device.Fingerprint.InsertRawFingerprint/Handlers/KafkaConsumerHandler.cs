using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Features.FingerPrint.Services;
using Device.FingerPrint.Kafka.Handlers;
using Device.FingerPrint.Model;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Sentry;
using System;

namespace Device.Fingerprint.InsertRawFingerprint.Handlers
{
    public class KafkaConsumerHandler : BackgroundService
    {
        private readonly IFingerPrintService fingerPrintService;
        
        private readonly ISentryClient sentryClient;

        public KafkaConsumerHandler(IFingerPrintService fingerPrintService, ISentryClient sentryClient)
        {
            this.sentryClient = sentryClient;
            this.fingerPrintService = fingerPrintService;            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new Thread(() => StartConsumerLoop(stoppingToken)).Start();

            return Task.CompletedTask;
        }
        private void StartConsumerLoop(CancellationToken cancellationToken)
        {
            try
            {
                //var callback = new CallBack<FingerPrintModel>(x =>
                //       {
                //           var result = x.Select(a => TranformFingerPrint(a));

                //           fingerPrintService.InsertListFingerPrint(result);
                //       }
                // );

                //requestTimeConsumer.StartConsumerLoop(cancellationToken, callback);
            }
            catch (Exception ex)
            {
                sentryClient.CaptureException(ex);
            }
        }

        private FingerPrintEntity TranformFingerPrint(FingerPrintModel model)
        {
            return new FingerPrintEntity
            {
                Attributes = model.Attributes,
                CookieId = model.CookieId,
                DeviceCode = model.DeviceCode,
                DeviceInfo = model.DeviceInfo,
                FingerPrint = model.FingerPrint,
                SessionKey = model.SessionKey,
                SubscriberId = model.SubscriberId,
                Flagged = model.Flagged
            };
        }

    }
}
