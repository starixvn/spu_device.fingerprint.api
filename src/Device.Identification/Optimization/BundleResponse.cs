namespace Device.Identification.Optimization
{
    public class BundleResponse(string content)
    {
        public string Content { get; private set; } = content;
    }
}
