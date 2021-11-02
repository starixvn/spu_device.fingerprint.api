using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Ultilities
{
    public class BundleFile
    {
        private readonly string virtualPath;
        private readonly int subscriberId;

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
