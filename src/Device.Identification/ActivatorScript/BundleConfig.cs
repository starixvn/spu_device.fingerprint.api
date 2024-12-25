namespace Device.Identification.ActivatorScript
{
    using Device.Identification.Optimization;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.FileProviders;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    public class BundleConfig
    {
        private readonly IMemoryCache _cache;
        private readonly IFileProvider _fileProvider;

        private readonly Dictionary<string, List<string>> _bundleDefinitions;

        public BundleConfig(IFileProvider fileProvider, IMemoryCache cache)
        {
            _cache = cache;
            _fileProvider = fileProvider;

            _bundleDefinitions = new Dictionary<string, List<string>>
            {
                { "config", new List<string> { "/fp.js", "/Detecas.Config.js" } },
                { "api", new List<string> { "/Detecas.Api.js" } },
                { "common", new List<string> {}},
                { "token", new List<string> { "/Detecas.ConfigToken.js" } }
            };

            LoadCommonScriptsFromDirectory();
        }

        public static bool EnableOptimization
        {
            get { return BundleTable.EnableOptimization; }
        }

        public void RegisterBundles()
        {
            foreach (var bundleName in _bundleDefinitions.Keys)
            {
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

        public string GetBundle(string bundleName)
        {
            if (!_bundleDefinitions.TryGetValue(bundleName, out var files))
            {
                throw new InvalidOperationException($"Bundle '{bundleName}' is not defined.");
            }


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


            var result = _cache.GetOrCreate($"Bundle_{bundleName}", entry =>
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

            return result ?? string.Empty;
        }

        public void LoadCommonScriptsFromDirectory()
        {
            var utilityFiles = new List<string>{
                                                "/Detecas.Base64.js",
                                                "/Detecas.Extensions.js",
                                                "/Detecas.Hex.js",
                                                "/Detecas.JSON.js",
                                                "/Detecas.Md5.js",
                                                "/Detecas.Rc4.js",
                                                "/Detecas.Sha1.js",
                                                "/Detecas.ua-parser-js.js",
                                            };

            AddFilesToBundle("common", utilityFiles);

            var commonExtraFiles = new List<string>{
                                                "/Detecas.BotD.js",
                                                "/Detecas.Debug.js",
                                                "/Detecas.Geolocation.js",
                                                "/Detecas.Common.js",
                                                "/Detecas.Cookie.js",
                                                "/Detecas.GPU.js",
                                                "/Detecas.WebGL.js",
                                                "/Detecas.RTC.js",
                                                "/Detecas.UserAgent.js",
                                                "/Detecas.Fonts.js",
                                                "/Detecas.Browser.js",
                                                "/Detecas.Device.js",
                                                "/Detecas.DeviceDetail.js",
                                                "/Detecas.Storages.js",
                                                "/Detecas.Core.js"
                                            };

            AddFilesToBundle("common", commonExtraFiles);
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
