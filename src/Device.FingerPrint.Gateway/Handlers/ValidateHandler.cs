
using Device.FingerPrint.Gateway.Model;
using Device.FingerPrint.Gateway.Services;
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
            if (request != null && request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(content);

                    if (transactionRequest != null && !string.IsNullOrWhiteSpace(transactionRequest?.Subscriber) && !string.IsNullOrWhiteSpace(transactionRequest.Account))
                    {
                        if (await this.subscriberService.ValidateAsync(transactionRequest?.Subscriber))
                        {
                            return await base.SendAsync(request, cancellationToken);
                        }
                    }                    
                }
            }

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }
    }
}
