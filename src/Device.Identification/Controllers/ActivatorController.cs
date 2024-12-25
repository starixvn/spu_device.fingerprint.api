namespace Device.Identification.Controllers
{
    using Device.Identification.ActivatorScript;
    using Device.Identification.Services;
    using Fanex.Logging;
    using Microsoft.AspNetCore.Mvc;
    using System;

    [ApiController]
    public class ActivatorController(IAppSettings appSettings
                                , IActivatorService activatorService
                                , IEcService ecService
                                , ActivatorScriptFactory scriptFactory
                                , HttpCachingWrapper httpCachingWrapper
    ) : ControllerBase
    {
        private readonly IAppSettings _appSettings = appSettings;
        private readonly IActivatorService _activatorService = activatorService;
        private readonly IEcService _ecService = ecService;
        private readonly ActivatorScriptFactory _scriptFactory = scriptFactory;
        private readonly HttpCachingWrapper _httpCachingWrapper = httpCachingWrapper;

        [HttpGet("activator.ashx")]
        public async Task ActivatorAshx()
        {
            try
            {
                await _activatorService.Execute();
            }
            catch (Exception ex)
            {
                LogService.Error(Utilities.GetLogMessage(ex));
            }
        }

        [HttpGet("tkexp.ashx")]
        public async Task TKexpAshx()
        {
            try
            {
                var subscriberScript = _scriptFactory.CreateSubscriberTokenScript();

                await subscriberScript.WriteScript(Response);
            }
            catch (Exception ex)
            {
                LogService.Error(Utilities.GetLogMessage(ex));
            }
        }

        [HttpGet("comm.ashx")]
        public async Task CommAshx()
        {
            try
            {
                var subscriberScript = _scriptFactory.CreateSubscriberCommonScript();
                _httpCachingWrapper.UpdateCachingWithExpiredSeconds(_appSettings.CachingExpiredSeconds);

                await subscriberScript.WriteScript(Response);
            }
            catch (Exception ex)
            {
                LogService.Error(Utilities.GetLogMessage(ex));
            }
        }

        [HttpGet("ec.ashx")]
        public async Task EcAshx()
        {
            try
            {
                await _ecService.Execute();
            }
            catch (Exception ex)
            {
                LogService.Error(Utilities.GetLogMessage(ex));
                await ResponseEmptyContent();
            }
        }

        private async Task ResponseEmptyContent()
        {
            var content = string.Empty;

            await Response.WriteAsync(content + "\n");
        }
    }
}
