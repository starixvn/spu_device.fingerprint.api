namespace Device.Fingerprint.InternalAPI.Model
{
    public class RedisStringRequest
    {
        public int DbIndex { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public long? ExpireSeconds { get; set; }
    }

    public class RedisDeleteRequest
    {
        public int DbIndex { get; set; }
        public string Key { get; set; } = string.Empty;
    }
}