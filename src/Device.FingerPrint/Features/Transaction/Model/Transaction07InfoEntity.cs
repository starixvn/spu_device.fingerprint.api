namespace Device.FingerPrint.Features.Transaction.Model
{
    public class Transaction07InfoEntity
    {
        public ulong TransID { get; set; }

        public int SubscriberID { get; set; }

        public ulong AccountID { get; set; }

        public string LoginName { get; set; }

        public ulong DeviceCodeID { get; set; }

        public ulong DeviceFingerprintID { get; set; }

        public ulong DeviceID { get; set; }

        public int DeviceStatus { get; set; }

        public string UserAgentKey { get; set; }

        public ulong FP_DeviceMappingID { get; set; }

        public ulong FP_DeviceID { get; set; }

        public int FP_DeviceStatus { get; set; }
    }
}
