using FPS.Device.FingerPrint.Net.Framework.Client.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Fps.Device.FingerPrint.Net.Framework.Client
{
    public interface IFpsService
    {
        void InsertTransaction(string action, string loginName, DateTime loginTime);

        void InsertTransaction(string action, string loginName, long custId, long subAccId, DateTime loginTime);

        string GetScriptUrl();
    }

    public class FpsService : IFpsService
    {
        private readonly int subscriberId;
        private readonly string apiUrl;
        private readonly string sessionKey;
        private readonly string transactionUrl;

        public FpsService(int subscriberId, string apiUrl)
        {
            this.subscriberId = subscriberId;
            this.apiUrl = apiUrl;
            this.sessionKey = Guid.NewGuid().ToString("N");

            transactionUrl = "http:" + GetUrl(apiUrl, "api/transaction");
        }

        public void InsertTransaction(string action, string loginName, DateTime loginTime)
        {
            if (!string.IsNullOrWhiteSpace(loginName) && subscriberId != 0)
            {
                var userAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var clientIp = GetIP();
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberId = subscriberId,
                    UserAgent = userAgent,
                    ClientIp = clientIp,
                    TransTime = DateTime.Now,
                    Url = HttpContext.Current.Request.Url.AbsoluteUri,
                    Action = action,
                    TransKey = loginName + "#" + loginTime.Ticks,
                    CustId = 0,
                    SubAccId = 0
                };
                var cancelToken = new CancellationToken();

                PostBasicAsync(transactionUrl, transaction, cancelToken).Wait();
            }
        }

        public void InsertTransaction(string action, string loginName, long custId, long subAccId, DateTime loginTime)
        {
            if (!string.IsNullOrWhiteSpace(loginName) && subscriberId != 0)
            {
                var userAgent = HttpContext.Current.Request.Headers["User-Agent"].ToString();
                var clientIp = GetIP();
                var transaction = new TransactionModel
                {
                    LoginName = loginName,
                    SessionKey = sessionKey,
                    SubscriberId = subscriberId,
                    UserAgent = userAgent,
                    ClientIp = clientIp,
                    TransTime = DateTime.UtcNow,
                    Url = HttpContext.Current.Request.Url.AbsoluteUri,
                    Action = action,
                    TransKey = loginName + "#" + loginTime.Ticks,
                    CustId = custId,
                    SubAccId = subAccId
                };
                var cancelToken = new CancellationToken();

                PostBasicAsync(transactionUrl, transaction, cancelToken).Wait();
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

        private static async Task PostBasicAsync(string url, object content, CancellationToken cancellationToken)
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
            return GetUrl(apiUrl, $"api/scr/{sessionKey}/{subscriberId}");
        }
    }
}
