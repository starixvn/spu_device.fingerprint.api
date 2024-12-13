namespace Device.Identification.Optimization
{
    public class BundleItem
    {
        public BundleItem(string virtualPath)
        {
            VirtualPath = virtualPath;
        }

        public string VirtualPath { get; private set; }

        public virtual string GetContent(BundleContext context)
        {
            var bundleFile = new BundleFile(VirtualPath, context);
            return bundleFile.Content;
        }
    }
}
