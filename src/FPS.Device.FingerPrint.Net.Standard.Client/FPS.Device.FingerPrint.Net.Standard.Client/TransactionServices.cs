using FPS.Device.FingerPrint.Net.Standard.Client.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FPS.Device.FingerPrint.Net.Standard.Client
{
    public interface ITransactionServices
    {
        void CheckTransaction(TransactionModel transactionModel);
    }
    public class TransactionServices : ITransactionServices
    {
        private readonly string subscriberName;
        private readonly string portalAPIUrl;
        private readonly string transactionUrl;

        public TransactionServices(string subscriberName, string portalAPIUrl)
        {

            this.portalAPIUrl = portalAPIUrl;

            this.subscriberName = subscriberName;

            this.transactionUrl = "http:" + GetUrl(this.portalAPIUrl, "api/transaction");
        }        

        private string GetUrl(string baseUrl, string relativeUrl)
        {
            var url = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";

            return url + relativeUrl;
        }

        private async Task PostBasicAsync<T>(string url, T content, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var json = JsonConvert.SerializeObject(content);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    client.Timeout = TimeSpan.FromSeconds(10);

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        public void CheckTransaction(TransactionModel transactionModel)
        {

            transactionModel.TransKey = transactionModel.LoginName + "#" + transactionModel.LoginTime.Ticks;
            transactionModel.SubscriberName = this.subscriberName;

            try
            {
                var cancelToken = new CancellationToken();

                _ = PostBasicAsync(this.transactionUrl, transactionModel, cancelToken);
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}
