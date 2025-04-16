namespace Device.Identification.ActivatorScript
{
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.FileProviders;
    using Newtonsoft.Json;
    using NUglify;
    using NUglify.JavaScript;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Transactions;

    public class BundleConfig
    {
        private readonly IMemoryCache _cache;
        private readonly IFileProvider _fileProvider;
        private bool _enableOptimization;

        private readonly Dictionary<string, List<string>> _bundleDefinitions;

        public BundleConfig(IFileProvider fileProvider, IMemoryCache cache)
        {
            _cache = cache;
            _fileProvider = fileProvider;

            _bundleDefinitions = new Dictionary<string, List<string>>
                {
                    { "creditConfig", new List<string> {"/CreditScript/Gfp.Config.js", "/CreditScript/Gfp.Index.js" } },
                    { "config", new List<string> { "/fp.js", "/Detecas.Config.js" } },
                    { "api", new List<string> { "/Detecas.Api.js" } },
                    { "common", new List<string> { "/CreditScript/Gfp.Common.js" } },
                    { "token", new List<string> { "/Detecas.ConfigToken.js" } }
               };
        }

        public bool EnableOptimization
        {
            get { return _enableOptimization; }
            set { _enableOptimization = value; }
        }       

        public string GetBundle(string bundleName)
        {
            if (!_bundleDefinitions.TryGetValue(bundleName, out var files))
            {
                throw new InvalidOperationException($"Bundle '{bundleName}' is not defined.");
            }

            return _cache.GetOrCreate($"Bundle_{bundleName}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);
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
                    var content = reader.ReadToEnd();

                    stringBuilder.AppendLine(content);
                }

                if (_enableOptimization)
                {
                    var codeSettings = new CodeSettings()
                    {
                        StrictMode = false,
                        PreserveImportantComments = false,
                        TermSemicolons = true
                    };

                    var originalCode = stringBuilder.ToString();

                    var minifyResult = Uglify.Js(originalCode, codeSettings);

                    if (!minifyResult.HasErrors)
                    {
                        return minifyResult.Code;
                    }
                    else
                    {
                        throw new Exception(JsonConvert.SerializeObject(minifyResult.Errors[0].ToString()));
                    }
                }

                return stringBuilder.ToString();
            }) ?? string.Empty;
        }
    }
}
