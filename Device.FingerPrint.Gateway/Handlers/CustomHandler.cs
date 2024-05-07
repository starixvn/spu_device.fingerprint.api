
using Device.FingerPrint.Gateway.Model;
using Device.FingerPrint.Gateway.Services;
using Newtonsoft.Json;
using System.Security.Policy;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class CustomHandler: DelegatingHandler
    {
        private readonly ISubscriberService subscriberService;

        public CustomHandler(ISubscriberService subscriberService) { 
            this.subscriberService = subscriberService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {                        
            var response = new HttpResponseMessage();

            var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(await request.Content.ReadAsStringAsync());                        
          
            if(!string.IsNullOrWhiteSpace(transactionRequest?.Subscriber))
            {
                if(await this.subscriberService.ValidateAsync(transactionRequest?.Subscriber))
                {
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    response.StatusCode = System.Net.HttpStatusCode.NotFound;                    
                }
            }
            else
            {
                response.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }

            return response;
        }
    }
}
