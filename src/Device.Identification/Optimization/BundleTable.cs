namespace Device.Identification.Optimization
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.FileProviders;
    using System;

    public static class BundleTable
    {
        private static BundleCollection? _instance;
        private static IFileProvider? _fileProvider;
        private static bool _enableOptimization;
        private static bool _enableOptimizationSet;

        public static BundleCollection Instance => _instance ?? throw new InvalidOperationException("BundleTable is not initialized. Call Initialize first.");

        public static bool EnableOptimization
        {
            get => _enableOptimizationSet && _enableOptimization;
            set
            {
                _enableOptimization = value;
                _enableOptimizationSet = true;
            }
        }

        public static IFileProvider FileProvider
        {
            get => _fileProvider ?? throw new InvalidOperationException("FileProvider is not set. Call Initialize first.");
            set => _fileProvider = value;
        }

        public static void Initialize(IMemoryCache memoryCache, IFileProvider fileProvider)
        {
            _instance = new BundleCollection(memoryCache);
            _fileProvider = fileProvider;
        }
    }
}
