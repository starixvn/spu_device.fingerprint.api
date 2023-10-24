namespace Device.FingerPrint.Model
{
    public class BotDModel
    {
        public BotDResult Result { get; set; }

        public string Detection { get; set; }

        public string Component { get; set; }
    }

    public class BotDResult
    {
        public bool Bot { get; set; }

        public string BotKind { get; set; }
    }
}
