
using Device.FingerPrint.Gateway.Model;
using Newtonsoft.Json;
using System.Text;

namespace Device.FingerPrint.Gateway.Handlers
{
    public class LogstashHandler : DelegatingHandler
    {        
        private readonly IAppSettings appSettings;
        public LogstashHandler(IAppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request != null && request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(content))
                {
                    var transactionRequest = JsonConvert.DeserializeObject<TransactionRequest>(content);

                    if (transactionRequest != null)
                    {
                        await PushLog(transactionRequest);
                    }                                     
                }

                return await base.SendAsync(request, cancellationToken);
            }

            return new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };
        }

        private async Task PushLog(TransactionRequest trans)
        {
            using (var httpClient = new HttpClient())
            {                
                var logMessage = new TransactionLogstashModel
                {
                    Timestamp = DateTime.UtcNow,
                    Level = "INFO",
                    Message = trans,
                    Tags = trans.Subscriber
                };
                
                var jsonLogMessage = JsonConvert.SerializeObject(logMessage);
                                
                var httpContent = new StringContent(jsonLogMessage, Encoding.UTF8, "application/json");

                
               _= await httpClient.PostAsync(appSettings.LogstashUrl, httpContent);
            }
        }
    }
}
