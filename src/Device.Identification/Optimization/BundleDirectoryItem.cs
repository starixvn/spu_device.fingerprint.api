namespace Device.Identification.Optimization
{
    using System.Linq;
    using System.Text;

    public class BundleDirectoryItem(string virtualPath, string searchPattern) : BundleItem(virtualPath)
    {
        public string SearchPattern { get; private set; } = searchPattern;

        public override string GetContent(BundleContext context)
        {
            var dirInfo = context.FileProvider.GetDirectoryContents(VirtualPath);
            var regex = PatternHelper.BuildWildcardRegex(SearchPattern);
            var bundleFiles = dirInfo
                .Where(file => !file.IsDirectory && regex.IsMatch(file.Name))
                .Select(file => new BundleFile(file));

            var stringBuilder = new StringBuilder();
            foreach (var bundleFile in bundleFiles)
            {
                stringBuilder.Append(bundleFile.Content);
            }

            return stringBuilder.ToString();
        }
    }
}
