using Device.FingerPrint.Model;

namespace Device.FingerPrint.QueueConsumer.Models
{
    public class TransactionLogstashModel
    {
        public RawTransactionModel Message { get; set; }

        public string Tags { get; set; }
    }
}
