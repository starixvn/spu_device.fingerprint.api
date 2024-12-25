namespace Device.Identification.Optimization
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Caching.Memory;

    public class HttpContextCache(IMemoryCache memoryCache) : IBundleCache
    {
        private readonly IMemoryCache _memoryCache = memoryCache;

        public bool IsEnabled(BundleContext context)
        {
            return context == null ? throw new ArgumentNullException(nameof(context)) : _memoryCache != null;
        }

        public BundleResponse Get(BundleContext context, Bundle bundle)
        {
            ArgumentNullException.ThrowIfNull(context);

            var cacheKey = bundle.GetCacheKey(context);
            _memoryCache.TryGetValue(cacheKey, out BundleResponse response);
            return response;
        }

        public void Put(BundleContext context, Bundle bundle, BundleResponse response)
        {
            ArgumentNullException.ThrowIfNull(context);

            ArgumentNullException.ThrowIfNull(bundle);

            ArgumentNullException.ThrowIfNull(response);

            var cacheKey = bundle.GetCacheKey(context);

            // Simulate dependency (list of virtual paths)
            var dependencyKeys = new List<string>();
            dependencyKeys.AddRange(bundle.Items.Select(f => f.VirtualPath));

            // Store in memory cache with a dependency-like key grouping (not directly supported in IMemoryCache)
            _memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(30));
        }
    }
}
