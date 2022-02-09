using Confluent.Kafka;
using Device.Fingerprint.WebAPI.Ultilities;
using Device.FingerPrint.Kafka.Handlers.Kafka.Handlers;
using Device.FingerPrint.Model;

namespace Device.Fingerprint.WebAPI.Services
{
    public interface IKafkaServices
    {
        public void SendTransactionToKafka(TransactionModel model);

        public void SendFingerprintToKafka(FingerPrintModel model);
    }
    public class KafkaServices : IKafkaServices
    {
        private readonly IAppSettings appSettings;
        private readonly IKafkaProducer<string, TransactionModel> kafkaProducerTrans;
        private readonly IKafkaProducer<string, FingerPrintModel> kafkaProducerFP;

        public KafkaServices(IAppSettings appSettings, IKafkaProducer<string, TransactionModel> kafkaProducerTrans, IKafkaProducer<string, FingerPrintModel> kafkaProducerFP)
        {
            this.appSettings = appSettings;
            this.kafkaProducerFP = kafkaProducerFP;
            this.kafkaProducerTrans = kafkaProducerTrans;
        }
        public void SendTransactionToKafka(TransactionModel model)
        {
            //using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
            //{
            //    var message = SerializationHelper.SerializeJson(model);
            //    var result = producer.ProduceAsync(appSettings.TransactionTopicName, new Message<Null, string> { Value = message })
            //                .GetAwaiter()
            //                .GetResult();
            //    if (result != null) return true;
            //    return false;
            //}

            kafkaProducerTrans.ProduceAsync(appSettings.TransactionTopicName, null, model);

        }
        public void SendFingerprintToKafka(FingerPrintModel model)
        {
            //using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
            //{
            //    var message = SerializationHelper.SerializeJson(model);
            //    producer.ProduceAsync(appSettings.FingerprintTopicName, new Message<Null, string> { Value = message })
            //        .GetAwaiter()
            //        .GetResult();
            //}

            kafkaProducerFP.ProduceAsync(appSettings.FingerprintTopicName, null, model);
        }
    }
}
