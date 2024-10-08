namespace Device.FingerPrint.TransactionAPI.Model
{
    public class MobileTransactionRequest
    {
        public string MobileAndroidCode { get; set; }

        public string MobileCustomCode { get; set; }

        public string MobileMediaDRMCode { get; set; }

        public bool IsMobileAppTrans { get; set; }

        public bool IsMobileEmulator { get; set; }
    }
}
