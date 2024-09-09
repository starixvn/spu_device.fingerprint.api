
using Device.FingerPrint.Gateway.Model;
using Device.FingerPrint.Gateway.Services;
using Fanex.Logging;
using Newtonsoft.Json;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class ValidateHandler : DelegatingHandler
    {
        private readonly ISubscriberService subscriberService;

        public ValidateHandler(ISubscriberService subscriberService)
        {
            this.subscriberService = subscriberService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (request != null && request.Content != null)
                {
                    var content = await request.Content.ReadAsStringAsync();

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(content);

                        LogService.Info($"this is log from gateway api: {transactionRequest.Subscriber}");

                        if (transactionRequest != null && !string.IsNullOrWhiteSpace(transactionRequest?.Subscriber) && !string.IsNullOrWhiteSpace(transactionRequest.Account))
                        {
                            if (await this.subscriberService.ValidateAsync(transactionRequest?.Subscriber))
                            {
                                return await base.SendAsync(request, cancellationToken);
                            }
                        }
                    }
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
