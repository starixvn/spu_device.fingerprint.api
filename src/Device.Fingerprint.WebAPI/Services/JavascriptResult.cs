using Microsoft.AspNetCore.Mvc;

namespace Device.Fingerprint.WebAPI.Services
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
