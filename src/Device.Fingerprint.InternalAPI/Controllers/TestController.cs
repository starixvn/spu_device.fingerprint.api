using Device.FingerPrint.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Device.Fingerprint.InternalAPI.Controllers
{
    [Route("api/test")]
    [ApiController]
    public class TestController(IAppSettings appSettings) : ControllerBase
    {
        private readonly IAppSettings appSettings = appSettings;

        [HttpGet("check-vault")]
        [AllowAnonymous]
        public ActionResult<string> CheckVault()
        {
            if (!appSettings.IsVaultEnable)
            {
                return Ok("Vault is disabled.");
            }

            return Ok("Vault is enabled.");
        }
    }
}
