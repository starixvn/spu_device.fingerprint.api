namespace Device.Identification.Optimization
{
    public interface IBundleCache
    {
        BundleResponse Get(BundleContext context, Bundle bundle);

        bool IsEnabled(BundleContext context);

        void Put(BundleContext context, Bundle bundle, BundleResponse response);
    }
}
