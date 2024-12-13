namespace Device.Identification.Optimization
{
    public class Bundle
    {
        private IBundleBuilder _builder;

        protected Bundle()
        {
            Items = new List<BundleItem>();
            ConcatenationToken = Environment.NewLine;
        }

        public Bundle(string virtualPath): this()
        {
            VirtualPath = virtualPath;
        }

        public string VirtualPath { get; private set; }

        public string ConcatenationToken { get; private set; }

        public IList<BundleItem> Items { get; private set; }

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
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            IBundleCache cache = context.BundleCollection.Cache;
            BundleResponse bundleResponse = cache.Get(context, this);
            return bundleResponse;
        }

        public void UpdateCache(BundleContext context, BundleResponse response)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            IBundleCache cache = context.BundleCollection.Cache;
            if (cache.IsEnabled(context))
            {
                cache.Put(context, this, response);
            }
        }

        public string GetCacheKey(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return string.Concat("Fanex.DI.Web.Optimization.Bundle:", context.BundleVirtualPath);
        }

        public BundleResponse GenerateBundleResponse(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var bundleContent = Builder.BuildBundleContent(this, context);
            return new BundleResponse(bundleContent);
        }
    }
}
