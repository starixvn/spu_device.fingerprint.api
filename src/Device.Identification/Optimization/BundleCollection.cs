using Microsoft.Extensions.Caching.Memory;
using System.Collections;

namespace Device.Identification.Optimization
{
    public class BundleCollection(IMemoryCache memoryCache) : IEnumerable<Bundle>
    {
        private readonly Dictionary<string, Bundle> _bundles = new(StringComparer.OrdinalIgnoreCase);

        public IBundleCache Cache { get; set; } = new HttpContextCache(memoryCache);

        public IEnumerator<Bundle> GetEnumerator()
        {
            return _bundles.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Bundle bundle)
        {
            ArgumentNullException.ThrowIfNull(bundle);

            _bundles.Add(bundle.VirtualPath, bundle);
        }

        public Bundle GetBundleFor(string bundleVirtualPath)
        {
            return _bundles[bundleVirtualPath];
        }
    }
}
