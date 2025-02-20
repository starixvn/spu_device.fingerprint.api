namespace Device.FingerPrint.Gateway.Model
{
    public class MobileTransactionModel
    {
        public string CustomCode { get; set; }

        public string Manufacturer { get; set; }

        public DateTime LoginTime { get; set; }

        public string Timezone { get; set; }

        public string SdkVersion { get; set; }

        public string SystemLanguage { get; set; }

        public string OS { get; set; }

        public string Brand { get; set; }

        public string Country { get; set; }

        public string ModelName { get; set; }

        public string Action { get; set; }

        public bool IsEmulator { get; set; }

        public string Subscriber { get; set; }

        public string MachineCode { get; set; }

        public string Exceptions { get; set; }

        public string ActionResult { get; set; }

        public string Account { get; set; }

        public DateTime TransDate { get; set; }
    }
}
