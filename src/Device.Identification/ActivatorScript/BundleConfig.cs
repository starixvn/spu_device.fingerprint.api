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

        private readonly Dictionary<string, List<string>> _bundleDefinitions;

        public BundleConfig(IMemoryCache cache, IWebHostEnvironment env)
        {
            _cache = cache;
            _fileProvider = env.WebRootFileProvider ?? throw new ArgumentNullException(nameof(env.WebRootFileProvider));

            // Định nghĩa các bundle
            _bundleDefinitions = new Dictionary<string, List<string>>
            {
                { "config", new List<string> { "/ScriptsDetecas/fp.js", "/ScriptsDetecas/Detecas.Config.js" } },
                { "api", new List<string> { "/ScriptsDetecas/Detecas.Api.js" } },
                { "common", new List<string> {
                                                "/ScriptsDetecas/Detecas.BotD.js",
                                                "/ScriptsDetecas/Detecas.Debug.js",
                                                "/ScriptsDetecas/Detecas.Geolocation.js",
                                                "/ScriptsDetecas/Detecas.Common.js",
                                                "/ScriptsDetecas/Detecas.Cookie.js",
                                                "/ScriptsDetecas/Detecas.GPU.js",
                                                "/ScriptsDetecas/Detecas.WebGL.js",
                                                "/ScriptsDetecas/Detecas.RTC.js",
                                                "/ScriptsDetecas/Detecas.UserAgent.js",
                                                "/ScriptsDetecas/Detecas.Fonts.js",
                                                "/ScriptsDetecas/Detecas.Browser.js",
                                                "/ScriptsDetecas/Detecas.Device.js",
                                                "/ScriptsDetecas/Detecas.DeviceDetail.js",
                                                "/ScriptsDetecas/Detecas.Storages.js",
                                                "/ScriptsDetecas/Detecas.Core.js"
                                            }
                },
                { "token", new List<string> { "/ScriptsDetecas/Detecas.ConfigToken.js" } }
            };

            LoadCommonScriptsFromDirectory();
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

        public void LoadCommonScriptsFromDirectory()
        {
            const string directoryPath = "/ScriptsUtility";
            var filesFromDirectory = GetJavaScriptFilesFromDirectory(directoryPath);

            AddFilesToBundle("common", filesFromDirectory);

            var extraFiles = new List<string>{
                                                "/ScriptsDetecas/Detecas.BotD.js",
                                                "/ScriptsDetecas/Detecas.Debug.js",
                                                "/ScriptsDetecas/Detecas.Geolocation.js",
                                                "/ScriptsDetecas/Detecas.Common.js",
                                                "/ScriptsDetecas/Detecas.Cookie.js",
                                                "/ScriptsDetecas/Detecas.GPU.js",
                                                "/ScriptsDetecas/Detecas.WebGL.js",
                                                "/ScriptsDetecas/Detecas.RTC.js",
                                                "/ScriptsDetecas/Detecas.UserAgent.js",
                                                "/ScriptsDetecas/Detecas.Fonts.js",
                                                "/ScriptsDetecas/Detecas.Browser.js",
                                                "/ScriptsDetecas/Detecas.Device.js",
                                                "/ScriptsDetecas/Detecas.DeviceDetail.js",
                                                "/ScriptsDetecas/Detecas.Storages.js",
                                                "/ScriptsDetecas/Detecas.Core.js"
                                            };

            AddFilesToBundle("common", extraFiles);
        }

        private List<string> GetJavaScriptFilesFromDirectory(string directoryPath)
        {
            var directoryContents = _fileProvider.GetDirectoryContents(directoryPath);

            if (!directoryContents.Exists)
            {
                throw new DirectoryNotFoundException($"Directory '{directoryPath}' not found.");
            }

            return directoryContents
                .Where(file => !file.IsDirectory && file.Name.EndsWith(".js"))
                .Select(file => $"{directoryPath}/{file.Name}")
                .ToList();
        }

        private void AddFilesToBundle(string bundleName, List<string> files)
        {
            if (_bundleDefinitions != null && _bundleDefinitions.TryGetValue(bundleName, out List<string>? value))
            {
                foreach (var file in files)
                {
                    if (!value.Contains(file))
                    {
                        value.Add(file);
                    }
                }
            }
        }
    }
}
