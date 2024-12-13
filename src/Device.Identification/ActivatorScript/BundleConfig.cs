namespace Device.Identification.ActivatorScript
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.FileProviders;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class BundleConfig
    {
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _env;
        private readonly IFileProvider _fileProvider;

        public BundleConfig(IMemoryCache cache, IWebHostEnvironment env)
        {
            _cache = cache;
            _env = env;
            _fileProvider = env.WebRootFileProvider;
        }

        private readonly Dictionary<string, List<string>> _bundleDefinitions = new()
    {
        { "config", new List<string> { "/ScriptsDetecas/fp.js", "/ScriptsDetecas/Detecas.Config.js" } },
        { "api", new List<string> { "/ScriptsDetecas/Detecas.Api.js" } },
        { "common", new List<string>
            {
                "/ScriptsUtility/file1.js",
                "/ScriptsUtility/file2.js",
                "/ScriptsDetecas/Detecas.Core.js",
                "/ScriptsDetecas/Detecas.Device.js"
            }
        },
        { "token", new List<string> { "/ScriptsDetecas/Detecas.ConfigToken.js" } }
    };

        public string GetBundle(string bundleName)
        {
            if (!_bundleDefinitions.TryGetValue(bundleName, out var files))
            {
                throw new InvalidOperationException($"Bundle '{bundleName}' is not defined.");
            }

            return _cache.GetOrCreate($"Bundle_{bundleName}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var stringBuilder = new StringBuilder();

                foreach (var file in files)
                {
                    var filePath = _fileProvider.GetFileInfo(file);
                    if (!filePath.Exists)
                    {
                        throw new FileNotFoundException($"File '{file}' in bundle '{bundleName}' does not exist.");
                    }

                    using var stream = filePath.CreateReadStream();
                    using var reader = new StreamReader(stream);
                    stringBuilder.AppendLine(reader.ReadToEnd());
                }

                return stringBuilder.ToString();
            });
        }
        public void RegisterBundles()
        {
            foreach (var bundleName in _bundleDefinitions.Keys)
            {
                // Chỉ cần gọi `GetBundle` để tạo cache
                try
                {
                    GetBundle(bundleName);
                    Console.WriteLine($"Bundle '{bundleName}' registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error registering bundle '{bundleName}': {ex.Message}");
                }
            }
        }
    }
}
