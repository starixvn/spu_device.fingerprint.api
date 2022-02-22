using Device.FingerPrint.Features.Offset.Services;
using Device.FingerPrint.Features.RawTransaction.Services;
using Device.FingerPrint.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Device.Fingerprint.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChunkController : ControllerBase
    {
        private readonly IRawTransactionService rawTransactionServices;
        private readonly IOffsetService offsetService;
        public ChunkController(IRawTransactionService rawTransactionServices, IOffsetService offsetService)
        {
            this.rawTransactionServices = rawTransactionServices;
            this.offsetService = offsetService;
        }

        // GET: api/<ChunkController>
        [HttpGet("get-raw-trans")]
        public IEnumerable<RawTransactionModel> GetRawTrans(int BatchSize)
        {
            var offsetRawTrans = offsetService.GetOffset("RawTransaction");
            var offset = offsetRawTrans.First();
            var rawTrans = rawTransactionServices.GetRawTrans(BatchSize, offset.OffsetValue);
            return rawTrans;
        }
    }
}
