using Device.FingerPrint.Gateway.Model;
using Microsoft.AspNetCore.Mvc;

namespace Device.FingerPrint.Gateway.Controllers
{

    [ApiController]
    public class TransactionController : ControllerBase
    {
        [HttpPost("FPS/CheckTransaction")]
        public IActionResult CheckTransaction(TransactionRequest transRequest)
        {
            var aa = transRequest.Account;

            return Ok(aa);
        }
    }
}
