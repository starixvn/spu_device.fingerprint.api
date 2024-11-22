
using Device.FingerPrint.Gateway.Model;
using Device.FingerPrint.Gateway.Services;
using Fanex.Logging;
using Newtonsoft.Json;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class MobileRequestHandler : DelegatingHandler
    {
        private readonly ISubscriberService subscriberService;

        private static readonly HashSet<string> DebugAccounts = new HashSet<string> { "member19" };

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
                        var transactionRequest = JsonConvert.DeserializeObject<MobileTransactionRequest>(content);

                        if (transactionRequest != null)
                        {
                            byte[] base64DecodedBytes = Convert.FromBase64String(transactionRequest.Transaction);

                            string decodedString = System.Text.Encoding.UTF8.GetString(base64DecodedBytes);

                            var transaction = JsonConvert.DeserializeObject<TransactionRequest>(decodedString);

                            if (transactionRequest != null && !string.IsNullOrWhiteSpace(transaction?.Subscriber) && !string.IsNullOrWhiteSpace(transaction.Account))
                            {
                                if (DebugAccounts.Contains(transaction.Account.ToLower()))
                                {
                                    _ = LogService.InfoAsync($"Gateway Debug for Account:{transaction.Account}\n {JsonConvert.SerializeObject(transaction)}");
                                }

                                if (await this.subscriberService.ValidateAsync(transaction.Subscriber))
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
