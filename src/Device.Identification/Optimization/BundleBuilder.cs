using Microsoft.Ajax.Utilities;
using System.Text;

namespace Device.Identification.Optimization
{
    public interface IBundleBuilder
    {
        string BuildBundleContent(Bundle bundle, BundleContext context);
    }

    public class BundleBuilder : IBundleBuilder
    {
        private static readonly Minifier _minifier = new();

        static BundleBuilder()
        {
            Instance = new BundleBuilder();
        }

        public static IBundleBuilder Instance { get; private set; }

        public string BuildBundleContent(Bundle bundle, BundleContext context)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            if (context == null) throw new ArgumentNullException(nameof(context));

            var concatenationToken = bundle.ConcatenationToken ?? Environment.NewLine;

            var stringBuilder = new StringBuilder();
            foreach (var bundleItem in bundle.Items)
            {
                var bundleItemContent = bundleItem.GetContent(context);

                // Minify content if optimization is enabled
                if (context.EnableOptimization)
                {
                    bundleItemContent = _minifier.MinifyJavaScript(bundleItemContent);
                }

                stringBuilder.Append(bundleItemContent);
                stringBuilder.Append(concatenationToken);
            }

            return stringBuilder.ToString();
        }
    }

}
