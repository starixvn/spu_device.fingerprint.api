using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceFingerprint.WebAPI.Services
{
    public class JavascriptResult : ContentResult
    {
        public JavascriptResult(string script)
        {
            this.Content = script;
            this.ContentType = "application/javascript";            
        }
    }
}
