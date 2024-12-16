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
        private readonly IFileProvider _fileProvider;

        // Sử dụng ReadOnlyDictionary để đảm bảo không bị thay đổi trong runtime
        private readonly IReadOnlyDictionary<string, List<string>> _bundleDefinitions;

        public BundleConfig(IMemoryCache cache, IWebHostEnvironment env)
        {
            _cache = cache;
            _fileProvider = env.WebRootFileProvider ?? throw new ArgumentNullException(nameof(env.WebRootFileProvider));

            // Định nghĩa các bundle
            _bundleDefinitions = new Dictionary<string, List<string>>
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
        }

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
                    var fileInfo = _fileProvider.GetFileInfo(file);
                    if (!fileInfo.Exists)
                    {
                        throw new FileNotFoundException($"File '{file}' in bundle '{bundleName}' does not exist.");
                    }

                    using var stream = fileInfo.CreateReadStream();
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
                try
                {
                    GetBundle(bundleName); // Chỉ cần gọi GetBundle để tạo cache
                    Console.WriteLine($"Bundle '{bundleName}' registered successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error registering bundle '{bundleName}': {ex.Message}");
                }
            }
        }

        public bool BundleExists(string bundleName)
        {
            return _bundleDefinitions.ContainsKey(bundleName);
        }
    }
}
