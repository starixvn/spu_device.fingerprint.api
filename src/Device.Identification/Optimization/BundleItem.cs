namespace Device.Identification.Optimization
{
    public class BundleItem(string virtualPath)
    {
        public string VirtualPath { get; private set; } = virtualPath;

        public virtual string GetContent(BundleContext context)
        {
            var bundleFile = new BundleFile(VirtualPath, context);
            return bundleFile.Content;
        }
    }
}
