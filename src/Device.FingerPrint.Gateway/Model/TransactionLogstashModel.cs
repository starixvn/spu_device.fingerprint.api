namespace Device.FingerPrint.Gateway.Model
{
    public class TransactionLogstashModel
    {
        public string Level { get; set; }

        public TransactionRequest Message { get; set; }

        public string Tags { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
