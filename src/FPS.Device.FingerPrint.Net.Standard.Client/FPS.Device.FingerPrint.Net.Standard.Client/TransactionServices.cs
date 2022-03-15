using System;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using FPS.Device.FingerPrint.Net.Standard.Client.Model;
using System.Threading;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace FPS.Device.FingerPrint.Net.Standard.Client
{
    public interface ITransactionServices
    {
        void Insert(string action, string loginName, DateTime loginTime, string userAgent, string hostUrl);
        void Insert(string action, string loginName, long custId, long subAccId, DateTime loginTime, string userAgent, string hostUrl);

        string GetScriptUrl();
    }
    public class TransactionServices : ITransactionServices
    {
        private readonly string subscriberName;
        private readonly string apiUrl;
        private readonly string sessionKey;
        private readonly string transactionUrl;

        public TransactionServices(string subscriberName, string apiUrl)
        {
            this.subscriberName = subscriberName;
            this.apiUrl = apiUrl;
            this.sessionKey = Guid.NewGuid().ToString("N");

            transactionUrl = "http:" + GetUrl(apiUrl, "api/transaction");
        }

        private string GetUrl(string baseUrl, string relativeUrl)
        {
            var url = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";

            return url + relativeUrl;
        }

        private async Task<string> GetIPAsync()
        {
            var hostName = System.Net.Dns.GetHostName();
            var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);

            return ips[2].ToString();
        }
        public string GetScriptUrl()
        {
            return GetUrl(apiUrl, $"api/scr/{sessionKey}/{subscriberName}");
        }

        /// <summary>
        /// Insert transaction
        /// </summary>
        /// <param name="action">User action</param>
        /// <param name="loginName">User name</param>
        /// <param name="loginTime">Login time</param>
        /// <param name="userAgent">UserAgent from httpcontext</param>
        /// <param name="hostUrl">Url from httpcontext</param>
        public void Insert(string action, string loginName, DateTime loginTime, string userAgent, string hostUrl)
        {
            if (!string.IsNullOrWhiteSpace(loginName) && !string.IsNullOrWhiteSpace(subscriberName))
            {
                var clientIp = GetIPAsync().Result;
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberName = subscriberName,
                    UserAgent = userAgent,
                    ClientIp = clientIp.ToString(),
                    Url = hostUrl,
                    Action = action,
                    TransKey = loginName + "#" + loginTime.Ticks,
                    CustId = 0,
                    SubAccId = 0
                };
                var cancelToken = new CancellationToken();

                _ = PostBasicAsync(transactionUrl, transaction, cancelToken);
            }
        }

        /// <summary>
        /// Insert transaction
        /// </summary>
        /// <param name="action">User action</param>
        /// <param name="loginName">User name</param>
        /// <param name="custId">Cust Id</param>
        /// <param name="subAccId">Sub Account Id</param>
        /// <param name="loginTime">Login time</param>
        /// <param name="userAgent">UserAgent from httpcontext</param>
        /// <param name="hostUrl">Url from httpcontext</param>
        public void Insert(string action, string loginName, long custId, long subAccId, DateTime loginTime, string userAgent, string hostUrl)
        {
            if (!string.IsNullOrWhiteSpace(loginName) && !string.IsNullOrWhiteSpace(subscriberName))
            {
                var clientIp = GetIPAsync().Result;
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberName = subscriberName,
                    UserAgent = userAgent,
                    ClientIp = clientIp.ToString(),
                    Url = hostUrl,
                    Action = action,
                    TransKey = loginName + "#" + loginTime.Ticks,
                    CustId = custId,
                    SubAccId = subAccId
                };
                var cancelToken = new CancellationToken();

                _ = PostBasicAsync(transactionUrl, transaction, cancelToken);
            }
        }

        private async Task PostBasicAsync(string url, object content, CancellationToken cancellationToken)
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
    }
}
