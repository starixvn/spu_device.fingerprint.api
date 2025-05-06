namespace Device.FingerPrint.Features.Transaction.Model
{
    public class TransactionValidateSuccessEntity
    {
        public ulong TransID { get; set; }

        public ulong AccountID { get; set; }

        public ulong DeviceCodeID { get; set; }

        public ulong DeviceFingerprintID { get; set; }

        public ulong DeviceID { get; set; }

        public int DeviceStatus { get; set; }

        public string UserAgentKey { get; set; }

        public ulong FP_DeviceMappingID { get; set; }

        public ulong FP_TaggingID { get; set; }

        public ulong FP_FingerPrintID { get; set; }

        public ulong FP_DeviceFingerprintAccountOldID { get; set; }

        public ulong FP_DeviceID { get; set; }

        public int FP_DeviceStatus { get; set; }
    }
}
