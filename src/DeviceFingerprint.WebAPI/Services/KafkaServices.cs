using Confluent.Kafka;
using DeviceFingerprint.WebAPI.Ultilities;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using FPS.Device.FingerPrint.Model;
using Sentry;
using System;

namespace DeviceFingerprint.WebAPI.Services
{
    public class KafkaServices
    {
        private readonly IAppSettings appSettings;
        private readonly ISentryClient sentryClient;
        private readonly ProducerConfig producerConfig;

        public KafkaServices(IAppSettings appSettings, ISentryClient sentryClient)
        {
            this.appSettings = appSettings;
            this.sentryClient = sentryClient;
            producerConfig = new ProducerConfig { BootstrapServers = appSettings.BootstrapServers };
        }
        public Object SendTransactionToKafka(TransactionModel model)
        {
            using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
            {
                try
                {
                    string message = SerializationHelper.SerializeJson(model);
                    return producer.ProduceAsync(appSettings.TransactionTopicName, new Message<Null, string> { Value = message })
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }
            }
            return null;
        }
        public Object SendFingerprintToKafka(FingerPrintEntity model)
        {
            using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
            {
                try
                {
                    string message = SerializationHelper.SerializeJson(model);
                    return producer.ProduceAsync(appSettings.FingerprintTopicName, new Message<Null, string> { Value = message })
                        .GetAwaiter()
                        .GetResult();
                }
                catch (Exception ex)
                {
                    sentryClient.CaptureException(ex);
                }
            }
            return null;
        }
    }
}
