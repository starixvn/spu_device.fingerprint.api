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
            kafkaProducerTrans.ProduceAsync(appSettings.ServerTransTopicName, null, model);
        }

        public void SendFingerprintToKafka(FingerPrintModel model)
        {
            kafkaProducerFP.ProduceAsync(appSettings.ClientTransTopicName, null, model);
        }
    }
}
