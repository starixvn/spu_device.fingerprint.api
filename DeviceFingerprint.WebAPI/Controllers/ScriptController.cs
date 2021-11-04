using DeviceFingerprint.WebAPI.Models;
using DeviceFingerprint.WebAPI.Ultilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DeviceFingerprint.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScriptController : ControllerBase
    {
        public IConfiguration Configuration;
        private readonly IWebHostEnvironment environment;

        public ScriptController(IWebHostEnvironment Environment, IConfiguration configuration)
        {
            this.environment = Environment;
            this.Configuration = configuration;
        }


        [HttpGet]
        public ActionResult<ScriptVariableModel> Get(int subscriberId)
        {
            var bundleFile = new BundleFile($"{environment.ContentRootPath}/Scripts/gfp.min.js");
            var scriptContent = bundleFile.Content;
            if (!string.IsNullOrWhiteSpace(scriptContent))
            {
                return new ScriptVariableModel
                {
                    JsScript = scriptContent
                };
            }
            return null;
        }
    }
}
