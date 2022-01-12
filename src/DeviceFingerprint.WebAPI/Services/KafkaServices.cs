using Confluent.Kafka;
using DeviceFingerprint.WebAPI.Ultilities;
using Fps.Device.FingerPrint.Features.FingerPrint.Model;
using FPS.Device.FingerPrint.Model;
using System;

namespace DeviceFingerprint.WebAPI.Services
{
    public class KafkaServices
    {
        private readonly IAppSettings appSettings;
        private readonly ProducerConfig producerConfig = new ProducerConfig { BootstrapServers = "localhost:9092" };

        public KafkaServices(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
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
                catch (Exception e)
                {
                    Console.WriteLine($"Oops, something went wrong: {e}");
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
                catch (Exception e)
                {
                    Console.WriteLine($"Oops, something went wrong: {e}");
                }
            }
            return null;
        }
    }
}
