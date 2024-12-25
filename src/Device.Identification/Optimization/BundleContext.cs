namespace Device.Identification.Optimization
{
    using System;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.FileProviders;

    public class BundleContext
    {
        public BundleContext(HttpContext context, BundleCollection collection, string bundleVirtualPath)
        {
            ArgumentNullException.ThrowIfNull(context);

            ArgumentNullException.ThrowIfNull(collection);

            ArgumentNullException.ThrowIfNull(bundleVirtualPath);

            HttpContext = context;
            BundleCollection = collection;
            BundleVirtualPath = bundleVirtualPath;
            FileProvider = BundleTable.FileProvider;
            EnableOptimization = BundleTable.EnableOptimization;
        }

        public bool EnableOptimization { get; set; }

        public HttpContext HttpContext { get; private set; }

        public BundleCollection BundleCollection { get; private set; }

        public string BundleVirtualPath { get; private set; }

        public IFileProvider FileProvider { get; set; }
    }

}
