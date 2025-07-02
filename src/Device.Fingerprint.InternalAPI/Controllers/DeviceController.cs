namespace Device.Fingerprint.InternalAPI.Controllers
{
    using Device.FingerPrint.Features.Devices.Model;
    using Device.FingerPrint.Features.Devices.Services;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly IDeviceServices deviceServices;
        private readonly ILogger logger;

        public DeviceController(IDeviceServices deviceServices, ILogger logger)
        {
            this.deviceServices = deviceServices;
            this.logger = logger;
        }

        [HttpPost("SearchDevice")]
        [AllowAnonymous]
        public ActionResult<List<SearchDeviceItemModel>> SearchDevice([FromBody] SearchDeviceParamModel param)
        {
            try
            {
                var data = deviceServices.SearchDevice(param);

                return Ok(data.ToList());

            }
            catch (Exception ex)
            {
                logger.Error("FPS Search Device info exception: " + ex.Message);
                return Ok(new List<SearchDeviceItemModel>());
            }
        }

        [HttpPost("GetAssDeviceInfoByIdSubscriberDCSCenter")]
        [AllowAnonymous]
        public ActionResult<GetAssDeviceInfoByIDSubscriberModel> GetAssDeviceInfoByIdSubscriberDCSCenter([FromBody] GetAssDeviceInfoByIDSubscriberParamModel param)
        {
            try
            {
                var data = deviceServices.GetAssDeviceInfoByIDSubscriber(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Ass Device Info by ID Subscriber exception: " + ex.Message);
                return Ok(new GetAssDeviceInfoByIDSubscriberModel());
            }
        }

        [HttpPost("GetAssAccountsByDeviceSubscriber")]
        [AllowAnonymous]
        public ActionResult<List<GetAssAccByDevSubscriberItemModel>> GetAssAccountsByDeviceSubscriber([FromBody] GetAssAccByDevSubscriberParamModel param)
        {
            try
            {
                var data = deviceServices.GetAssAccByDevSubscriber(param);

                return Ok(data.ToList());
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Ass Account by Device subscriber exception: " + ex.Message);
                return Ok(new List<GetAssAccByDevSubscriberItemModel>());
            }
        }

        [HttpPost("GetAssDevByDeviceSubscriber")]
        [AllowAnonymous]
        public ActionResult<List<GetAssDevByDevSubscriberItemModel>> GetAssDevByDeviceSubscriber([FromBody] GetAssDevByDevSubscriberParamModel param)
        {
            try
            {
                var data = deviceServices.GetAssDevByDevSubscriber(param);

                return Ok(data.ToList());
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Ass Device by Device subscriber exception: " + ex.Message);
                return Ok(new List<GetAssDevByDevSubscriberItemModel>());
            }
        }

        [HttpPost("GetAssMatrixByDeviceSubscriber")]
        [AllowAnonymous]
        public ActionResult<List<GetAssMatrixByDeviceSubscriberItemModel>> GetAssMatrixByDeviceSubscriber([FromBody] GetAssMatrixByDeviceSubscriberParamModel param)
        {
            try
            {
                var data = deviceServices.GetAssMatrixByDeviceSubscriber(param);

                return Ok(data.ToList());
            }
            catch (Exception ex)
            {

                logger.Error("FPS Get Ass Matrix by Device subscriber exception: " + ex.Message);
                return Ok(new List<GetAssMatrixByDeviceSubscriberItemModel>());
            }
        }

        [HttpPost("GetDevicesPerAccount")]
        [AllowAnonymous]
        public ActionResult<GetDevicesPerAccountResultModel> GetDevicesPerAccount([FromBody] GetDevicesPerAccountParamModel param)
        {
            try
            {
                var data = deviceServices.GetDevicesPerAccount(param);

                return Ok(data);
            }
            catch (Exception ex)
            {
                logger.Error("FPS Get Devices Per Account exception: " + ex.Message);
                return Ok(new GetDevicesPerAccountResultModel());
            }
        }
    }
}
