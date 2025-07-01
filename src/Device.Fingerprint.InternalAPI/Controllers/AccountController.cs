namespace Device.Fingerprint.InternalAPI.Controllers
{
    using Device.FingerPrint.Features.Account.Model;
    using Device.FingerPrint.Features.Account.Services;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountServices accountServices;
        private readonly ILogger logger;

        public AccountController(IAccountServices accountServices, ILogger logger)
        {
            this.accountServices = accountServices;
            this.logger = logger;
        }

        [HttpPost("SearchAccount")]
        [AllowAnonymous]
        public ActionResult<SearchAccountResultModel> SearchAccount([FromBody] SearchAccountParamModel param)
        {
            try
            {
                var data = accountServices.SearchAccount(param);

                return Ok(data);

            }
            catch (Exception ex)
            {
                logger.Error("FPS Search Account info exception: " + ex.Message);
                return Ok(new SearchAccountResultModel());
            }
        }

        [HttpPost("GetAssAccountInfoByCustSubscriber")]
        [AllowAnonymous]
        public ActionResult<GetAssAccountInfoByCustSubscriberModel> GetAssAccountInfoByCustSubscriber([FromBody] GetAssAccountInfoByCustSubscriberParamModel param)
        {
            try
            {
                var data = accountServices.GetAssAccountInfoByCustSubscriber(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Ass Account Info by Cust Subscriber exception: " + ex.Message);
                return Ok(new GetAssAccountInfoByCustSubscriberModel());
            }
        }

        [HttpPost("GetAssAccountsByCustSubscriber")]
        [AllowAnonymous]
        public ActionResult<GetAssAccountsByCustSubscriberResultModel> GetAssAccountsByCustSubscriber([FromBody] GetAssAccountsByCustSubscriberParamModel param)
        {
            try
            {
                var data = accountServices.GetAssAccountsByCustSubscriber(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Ass Accounts by Cust Subscriber exception: " + ex.Message);
                return Ok(new GetAssAccountsByCustSubscriberResultModel());
            }
        }

        [HttpPost("GetAssDevicesByCustSubscriber")]
        [AllowAnonymous]
        public ActionResult<GetAssDevicesByCustSubscriberResultModel> GetAssDevicesByCustSubscriber([FromBody] GetAssDevicesByCustSubscriberParamModel param)
        {
            try
            {
                var data = accountServices.GetAssDevicesByCustSubscriber(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Ass Devices by Cust Subscriber exception: " + ex.Message);
                return Ok(new GetAssDevicesByCustSubscriberResultModel());
            }
        }

        [HttpPost("GetAssMatrixAccountByCustSubscriber")]
        [AllowAnonymous]
        public ActionResult<GetAssMatrixAccountByCustSubscriberResultModel> GetAssMatrixAccountByCustSubscriber([FromBody] GetAssMatrixAccountByCustSubscriberParamModel param)
        {
            try
            {
                var data = accountServices.GetAssMatrixAccountByCustSubscriber(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Ass Account info by cust subscriber exception: " + ex.Message);
                return Ok(new GetAssMatrixAccountByCustSubscriberResultModel());
            }
        }

        [HttpPost("GetAccountsPerDevice")]
        [AllowAnonymous]
        public ActionResult<GetAccountsPerDeviceResultModel> GetAccountsPerDevice([FromBody] GetAccountPerDeviceParamModel param)
        {
            try
            {
                var data = accountServices.GetAccountsPerDevice(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Device By Account exception: " + ex.Message);
                return Ok(new GetAssMatrixAccountByCustSubscriberResultModel());
            }
        }
    }
}
