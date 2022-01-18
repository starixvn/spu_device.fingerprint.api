using Confluent.Kafka;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Features.FingerPrint.Model;
using Device.FingerPrint.Model;

namespace Device.Fingerprint.WebAPI.Services
{
    public interface IKafkaServices
    {
        public bool SendTransactionToKafka(TransactionModel model);
        public void SendFingerprintToKafka(FingerPrintEntity model);
    }
    public class KafkaServices : IKafkaServices
    {
        private readonly IAppSettings appSettings;
        private readonly ProducerConfig producerConfig;

        public KafkaServices(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
            producerConfig = new ProducerConfig { BootstrapServers = appSettings.BootstrapServers };
        }
        public bool SendTransactionToKafka(TransactionModel model)
        {
            using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
            {
                var message = SerializationHelper.SerializeJson(model);
                var result = producer.ProduceAsync(appSettings.TransactionTopicName, new Message<Null, string> { Value = message })
                            .GetAwaiter()
                            .GetResult();
                if (result != null) return true;
                return false;
            }
        }
        public void SendFingerprintToKafka(FingerPrintEntity model)
        {
            using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
            {
                var message = SerializationHelper.SerializeJson(model);
                producer.ProduceAsync(appSettings.FingerprintTopicName, new Message<Null, string> { Value = message })
                    .GetAwaiter()
                    .GetResult();
            }
        }
    }
}
