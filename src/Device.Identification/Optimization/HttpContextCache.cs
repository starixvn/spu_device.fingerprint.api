namespace Device.Identification.Optimization
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Caching.Memory;

    public class HttpContextCache : IBundleCache
    {
        private readonly IMemoryCache _memoryCache;

        public HttpContextCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public bool IsEnabled(BundleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return _memoryCache != null;
        }

        public BundleResponse Get(BundleContext context, Bundle bundle)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var cacheKey = bundle.GetCacheKey(context);
            _memoryCache.TryGetValue(cacheKey, out BundleResponse response);
            return response;
        }

        public void Put(BundleContext context, Bundle bundle, BundleResponse response)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (bundle == null)
            {
                throw new ArgumentNullException(nameof(bundle));
            }

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            var cacheKey = bundle.GetCacheKey(context);

            // Simulate dependency (list of virtual paths)
            var dependencyKeys = new List<string>();
            dependencyKeys.AddRange(bundle.Items.Select(f => f.VirtualPath));

            // Store in memory cache with a dependency-like key grouping (not directly supported in IMemoryCache)
            _memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(30));
        }
    }
}
