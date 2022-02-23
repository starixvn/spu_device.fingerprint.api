
namespace Device.FingerPrint.Model
{
    public class TransactionUpdateConfigModel
    {
        public string Json { get; set; }
        public int Retry { get; set; }
        public string OffsetName { get; set; }
        public long Offsetvalue { get; set; }
    }
}
