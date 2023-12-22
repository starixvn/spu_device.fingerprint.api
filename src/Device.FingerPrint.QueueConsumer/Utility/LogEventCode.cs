namespace Device.FingerPrint.QueueConsumer.Utility
{
    public static class LogEventCode
    {
        public const string Source = "RabbitMQ";

        public const string Destination = "DCS_RawTrans";

        public const string MethodTransformRawTran = "TransformRawTran";

        public const string MethodInsertTran = "InsertTran";
    }
}
