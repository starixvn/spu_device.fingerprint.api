namespace Device.FingerPrint.Model
{
    public class DeviceDetectorModel
    {
        public long Id { get; set; }

        public string ClientType { get; set; }

        public string ClientName { get; set; }

        public string ClientVersion { get; set; }

        public string OSName { get; set; }

        public string OSShortName { get; set; }

        public string OSVersion { get; set; }

        public string OSPlatform { get; set; }

        public string Device { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public string BotName { get; set; }

        public string BotCategory { get; set; }
    }
}
