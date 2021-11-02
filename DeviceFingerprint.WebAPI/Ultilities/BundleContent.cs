using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Ultilities
{
    public class BundleContent
    {        
        private readonly string scriptContent;

        public BundleContent(string scriptContent)
        {     
            this.scriptContent = scriptContent;
        }

        public string Content
        {
            get
            {
                var script = this.scriptContent.Replace("__key__", "wNvK6fr8JX+vVnYI0YrguI/PmrtH6B3fEm8YCMEBhYo=");

                return script;
            }
        }

        
    }
}
