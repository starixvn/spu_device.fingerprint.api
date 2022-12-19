using FPS.Device.FingerPrint.Net.Framework.Client.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace FPS.Device.FingerPrint.Net.Framework.Client
{
    public interface ITransactionServices
    {
        void Insert(string action, string loginName, DateTime loginTime);                

        void Insert(string action, string loginName, long custId, long subAccId, DateTime loginTime);
    }

    public class TransactionServices : ITransactionServices
    {
        private readonly string subscriberName;        
        private readonly string transactionUrl;        

        public TransactionServices(string subscriberName, string portalAPIUrl)
        {
            this.subscriberName = subscriberName;
            
            transactionUrl = "http:" + GetUrl(portalAPIUrl, "api/transaction");
        }

        private string GetUrl(string baseUrl, string relativeUrl)
        {
            var url = baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/";

            return url + relativeUrl;
        }

        private string GetIP()
        {
            var ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ip))
            {
                ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }

            return ip;
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

        public void Insert(string action, string loginName, long custId, long subAccId, DateTime loginTime)
        {
            if (string.IsNullOrWhiteSpace(loginName)) throw new ArgumentNullException(loginName, "Login Name cannot be null.");
            if (string.IsNullOrWhiteSpace(subscriberName)) throw new ArgumentNullException(loginName, "Subscriber Name cannot be null.");

            var userAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
            var clientIp = GetIP();

            var sessionKey = HttpContext.Current.Request.Form["__fdi"];         

            var transaction = new TransactionModel
            {
                LoginName = loginName,
                SessionKey = sessionKey,                
                SubscriberName = subscriberName,
                UserAgent = userAgent,
                ClientIp = clientIp,
                Url = HttpContext.Current.Request.UrlReferrer.AbsoluteUri,
                Action = action,
                TransKey = loginName + "#" + loginTime.Ticks,
                CustId = custId,
                SubAccId = subAccId
            };
            var cancelToken = new CancellationToken();

            _ = PostBasicAsync(transactionUrl, transaction, cancelToken);

            _ = PostBasicAsync(transactionUrl, transaction, cancelToken);
        }

        public void Insert(string action, string loginName, DateTime loginTime)
        {
            Insert(action, loginName, 0, 0, loginTime);
        }
    }
}
