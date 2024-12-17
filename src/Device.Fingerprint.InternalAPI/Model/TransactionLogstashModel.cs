using Device.FingerPrint.Model;

namespace Device.Fingerprint.InternalAPI.Model
{
    public class TransactionLogstashModel
    {
        public RawTransactionModel Message { get; set; }

        public string Tags { get; set; }

        public List<string> FingerprintAttributes { get; set; }

        public List<string> UnhandledAttributes { get; set; }
    }
}
