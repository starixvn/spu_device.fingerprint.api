
using Device.FingerPrint.Gateway.Model;
using Device.FingerPrint.Gateway.Services;
using Fanex.Logging;
using Newtonsoft.Json;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class MobileRequestHandler : DelegatingHandler
    {
        private readonly ISubscriberService subscriberService;

        public MobileRequestHandler(ISubscriberService subscriberService)
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
                        byte[] base64DecodedBytes = Convert.FromBase64String(content);

                        string decodedString = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);

                        var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(decodedString);

                        if (transactionRequest != null && !string.IsNullOrWhiteSpace(transactionRequest?.Subscriber) && !string.IsNullOrWhiteSpace(transactionRequest.Account))
                        {
                            if (await this.subscriberService.ValidateAsync(transactionRequest?.Subscriber))
                            {
                                return await base.SendAsync(request, cancellationToken);
                            }
                        }
                        else
                        {
                            return new HttpResponseMessage
                            {
                                StatusCode = System.Net.HttpStatusCode.BadRequest,
                                Content = new StringContent("Subscriber or Account cannot be null.")
                            };
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
