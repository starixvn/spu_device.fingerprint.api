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

        void Insert(string action, string loginName, long loginTime);

        void Insert(string action, string loginName, long custId, long subAccId, long loginTime);

        string GetScriptUrl();
    }

    public class TransactionServices : ITransactionServices
    {
        private readonly string subscriberName;
        private readonly string externalAPIUrl;        
        private readonly string sessionKey;
        private readonly string transactionUrl;

        public TransactionServices(string subscriberName, string externalAPIUrl)
        {
            this.subscriberName = subscriberName;
            this.externalAPIUrl = externalAPIUrl;
            this.sessionKey = Guid.NewGuid().ToString("N");

            transactionUrl = "http:" + GetUrl(externalAPIUrl, "api/transaction");
        }

        public TransactionServices(string subscriberName, string externalAPIUrl, string internalAPIUrl)
        {
            this.subscriberName = subscriberName;
            this.externalAPIUrl = externalAPIUrl;            
            this.sessionKey = Guid.NewGuid().ToString("N");

            transactionUrl = "http:" + GetUrl(string.IsNullOrWhiteSpace(internalAPIUrl) ? externalAPIUrl : internalAPIUrl, "api/transaction");
        }

        public void Insert(string action, string loginName, DateTime loginTime)
        {
            if (!string.IsNullOrWhiteSpace(loginName) && !string.IsNullOrWhiteSpace(subscriberName))
            {
                var userAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var clientIp = GetIP();                           
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberName = subscriberName,
                    UserAgent = userAgent,
                    ClientIp = clientIp,                    
                    Url = HttpContext.Current.Request.Url.AbsoluteUri,
                    Action = action,
                    TransKey = loginName + "#" + loginTime.Ticks,
                    CustId = 0,
                    SubAccId = 0
                };
                var cancelToken = new CancellationToken();

                _ = PostBasicAsync(transactionUrl, transaction, cancelToken);
            }
        }

        public void Insert(string action, string loginName, long custId, long subAccId, DateTime loginTime)
        {
            if (!string.IsNullOrWhiteSpace(loginName) && !string.IsNullOrWhiteSpace(subscriberName))
            {
                var userAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var clientIp = GetIP();                             
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberName = subscriberName,
                    UserAgent = userAgent,
                    ClientIp = clientIp,                    
                    Url = HttpContext.Current.Request.Url.AbsoluteUri,
                    Action = action,
                    TransKey = loginName + "#" + loginTime.Ticks,
                    CustId = custId,
                    SubAccId = subAccId
                };
                var cancelToken = new CancellationToken();

                _ = PostBasicAsync(transactionUrl, transaction, cancelToken);
            }
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

        public string GetScriptUrl()
        {
            return GetUrl(externalAPIUrl, $"api/scr/{sessionKey}/{subscriberName}");
        }

        public void Insert(string action, string loginName, long loginTime)
        {
            if (!string.IsNullOrWhiteSpace(loginName) && !string.IsNullOrWhiteSpace(subscriberName))
            {
                var userAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var clientIp = GetIP();
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberName = subscriberName,
                    UserAgent = userAgent,
                    ClientIp = clientIp,
                    Url = HttpContext.Current.Request.Url.AbsoluteUri,
                    Action = action,
                    TransKey = loginName + "#" + loginTime,
                    CustId = 0,
                    SubAccId = 0
                };
                var cancelToken = new CancellationToken();

                _ = PostBasicAsync(transactionUrl, transaction, cancelToken);
            }
        }

        public void Insert(string action, string loginName, long custId, long subAccId, long loginTime)
        {
            if (!string.IsNullOrWhiteSpace(loginName) && !string.IsNullOrWhiteSpace(subscriberName))
            {
                var userAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var clientIp = GetIP();
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberName = subscriberName,
                    UserAgent = userAgent,
                    ClientIp = clientIp,
                    Url = HttpContext.Current.Request.Url.AbsoluteUri,
                    Action = action,
                    TransKey = loginName + "#" + loginTime,
                    CustId = custId,
                    SubAccId = subAccId
                };
                var cancelToken = new CancellationToken();

                _ = PostBasicAsync(transactionUrl, transaction, cancelToken);
            }
        }
    }
}
