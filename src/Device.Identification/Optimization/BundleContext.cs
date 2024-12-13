namespace Device.Identification.Optimization
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.FileProviders;

    public class BundleContext
    {
        protected BundleContext()
        {
            EnableOptimization = BundleTable.EnableOptimization;
        }

        public BundleContext(HttpContext context, BundleCollection collection, string bundleVirtualPath)
            : this()
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (bundleVirtualPath == null)
            {
                throw new ArgumentNullException(nameof(bundleVirtualPath));
            }

            HttpContext = context;
            BundleCollection = collection;
            BundleVirtualPath = bundleVirtualPath;
            FileProvider = BundleTable.FileProvider;
        }

        public bool EnableOptimization { get; set; }

        public HttpContext HttpContext { get; private set; }

        public BundleCollection BundleCollection { get; private set; }

        public string BundleVirtualPath { get; private set; }

        public IFileProvider FileProvider { get; set; }
    }

}
