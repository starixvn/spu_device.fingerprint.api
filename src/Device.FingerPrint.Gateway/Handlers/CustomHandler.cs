
using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;
using System.Security.Policy;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class CustomHandler: DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {                        

            var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(request.Content.ReadAsStringAsync().Result);                        

            request.Headers.Add("Subscriber", transactionRequest?.Subscriber);
            
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
