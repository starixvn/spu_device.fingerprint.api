using FPS.Device.FingerPrint.Net.Standard.Client.Model;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net.Http;
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
        private readonly string transactionUrl;

        private readonly string subscriberName;

        public TransactionServices(string portalAPIUrl, string subscriberName)
        {
            this.transactionUrl = "http:" + GetUrl(portalAPIUrl, "api/transaction");

            this.subscriberName = subscriberName;
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

            var cancelToken = new CancellationToken();

            _ = PostBasicAsync<TransactionModel>(this.transactionUrl, transactionModel, cancelToken);
        }
    }
}
