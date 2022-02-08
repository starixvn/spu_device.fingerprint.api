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

        void InsertTransaction(string action, string loginName, long custId = 0, long subAccId = 0, string transKey = null, TimeSpan? timeout = null);               

        string GetScriptUrl();
    }

    public class FpsService : IFpsService
    {
        private readonly int subscriberId;
        private readonly string apiUrl;
        private readonly string sessionKey;

        public FpsService(int subscriberId, string apiUrl)
        {
            this.subscriberId = subscriberId;
            this.apiUrl = apiUrl;
            this.sessionKey = Guid.NewGuid().ToString("N");
        }

        public void InsertTransaction(string action, string loginName, long custId = 0, long subAccId = 0, string transKey = null, TimeSpan? timeout = null)
        {            
            if (!string.IsNullOrWhiteSpace(loginName) && subscriberId != 0)
            {                
                var url = "http:" + GetUrl(apiUrl, "api/fp/Transaction");                                
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
                    TransKey = transKey,
                    CustId = custId,
                    SubAccId = subAccId
                };
                var cancelToken = new CancellationToken();

                PostBasicAsync(url, transaction, cancelToken).Wait();
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
            return GetUrl(apiUrl, $"api/fp/scr/{sessionKey}/{subscriberId}");
        }
    }
}
