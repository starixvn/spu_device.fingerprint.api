namespace Device.Identification.Optimization
{
    public class BundleResponse
    {
        public BundleResponse(string content)
        {
            Content = content;
        }

        public string Content { get; private set; }
    }
}
