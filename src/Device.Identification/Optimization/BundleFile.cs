namespace Device.Identification.Optimization
{
    using System.IO;
    using Microsoft.Extensions.FileProviders;

    public class BundleFile
    {
        public BundleFile(IFileInfo file)
        {
            FileInfo = file;
        }

        public BundleFile(string virtualPath, BundleContext context)
        {
            FileInfo = context.FileProvider.GetFileInfo(virtualPath);
        }

        public IFileInfo FileInfo { get; private set; }

        public string Content
        {
            get
            {
                using (var stream = FileInfo.CreateReadStream())
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
