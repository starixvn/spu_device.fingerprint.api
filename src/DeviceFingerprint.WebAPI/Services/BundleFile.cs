using System.IO;

namespace DeviceFingerprint.WebAPI.Ultilities
{
    public class BundleFile
    {
        private readonly string virtualPath;        

        public BundleFile(string virtualPath)
        {
            this.virtualPath = virtualPath;
        }

        public string Content
        {
            get
            {
                using (var streamReader = new StreamReader(virtualPath))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
