using Fanex.Logging;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class IPCaptureHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IPCaptureHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (request != null && _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
                {
                    var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                    if(!string.IsNullOrEmpty(ipAddress))
                        request.Headers.Add("X-Forwarded-For", ipAddress);

                    return await base.SendAsync(request, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                LogService.Error(ex.Message, ex);
            }

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
    }
}
