namespace Device.Identification.Optimization
{
    public class Bundle(string virtualPath)
    {
        private IBundleBuilder? _builder;

        public string VirtualPath { get; private set; } = virtualPath;

        public string ConcatenationToken { get; private set; } = Environment.NewLine;

        public IList<BundleItem> Items { get; private set; } = [];

        public virtual IBundleBuilder Builder
        {
            get
            {
                if (_builder == null)
                {
                    return BundleBuilder.Instance;
                }

                return _builder;
            }

            set
            {
                _builder = value;
            }
        }

        public Bundle Include(params string[] virtualPaths)
        {
            foreach (var virtualPath in virtualPaths)
            {
                Items.Add(new BundleItem(virtualPath));
            }

            return this;
        }

        public Bundle IncludeDirectory(string directoryVirtualPath, string searchPattern)
        {
            Items.Add(new BundleDirectoryItem(directoryVirtualPath, searchPattern));
            return this;
        }

        public BundleResponse GetBundleResponse(BundleContext context)
        {
            BundleResponse bundleResponse = CacheLookup(context);
            if (bundleResponse == null)
            {
                bundleResponse = GenerateBundleResponse(context);
                UpdateCache(context, bundleResponse);
            }

            return bundleResponse;
        }

        public BundleResponse CacheLookup(BundleContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            IBundleCache cache = context.BundleCollection.Cache;
            BundleResponse bundleResponse = cache.Get(context, this);
            return bundleResponse;
        }

        public void UpdateCache(BundleContext context, BundleResponse response)
        {
            ArgumentNullException.ThrowIfNull(context);

            ArgumentNullException.ThrowIfNull(response);

            IBundleCache cache = context.BundleCollection.Cache;
            if (cache.IsEnabled(context))
            {
                cache.Put(context, this, response);
            }
        }

        public string GetCacheKey(BundleContext context)
        {
            return context == null
                ? throw new ArgumentNullException(nameof(context))
                : string.Concat("Fanex.DI.Web.Optimization.Bundle:", context.BundleVirtualPath);
        }

        public BundleResponse GenerateBundleResponse(BundleContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var bundleContent = Builder.BuildBundleContent(this, context);
            return new BundleResponse(bundleContent);
        }
    }
}
